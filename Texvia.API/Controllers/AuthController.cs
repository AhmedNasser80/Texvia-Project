using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Texvia.Domain.Conctracts;
using Texvia.Domain.Models;
using Texvia.Shared.Dtos;

namespace Texvia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        #region Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            #region ✅ 3.1 Register an Admin via API
            if (model.Role == "admin")
            {
                if (!User.Identity.IsAuthenticated || !User.IsInRole("admin"))
                    return Forbid();
            }
            else
            {
                model.Role = "user";
            }

            #endregion

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { code = "DuplicateUserName", description = "Email already taken." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name   // تخزين الاسم الحقيقي هنا
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
                return UnprocessableEntity(createResult.Errors);
            //3.2 Add an Admin via Backend Admin Interface or Database

            await _userManager.AddToRoleAsync(user, model.Role);

            var tokens = await _tokenService.GenerateTokensAsync(user);

            return Created("", new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,  
                    role = model.Role
                }
            });
        }

        #endregion
        #region Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials" });

            var tokens = await _tokenService.GenerateTokensAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    role = roles.FirstOrDefault()
                }
            });
        }

        #endregion
        #region Refresh Token
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto model)
        {
            var tokens = await _tokenService.RefreshTokenAsync(model.RefreshToken);
            if (tokens == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            return Ok(new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken
            });
        }
        #endregion
        #region Forget Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { message = "Email not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = $"https://yourfrontend.com/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";

            await SendEmailAsync(user.Email, "Reset Password", $"Reset your password using this link: {resetLink}");

            // لو في dev mode رجعه في الريسبونس
            var isDev = _configuration.GetValue<bool>("IsDevelopment");

            return Ok(new
            {
                message = "Password reset link generated.",
                resetLink = isDev ? resetLink : null // رجّع اللينك فقط في التطوير
            });
        }


        #endregion
        #region Send Email
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var isDev = _configuration.GetValue<bool>("IsDevelopment");

            if (isDev)
            {
                Console.WriteLine($"[DEV MODE] Sending email to: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {body}");
                await Task.CompletedTask;
            }
            else
            {
                var smtpClient = new System.Net.Mail.SmtpClient("smtp.yourprovider.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("your@email.com", "your-password"),
                    EnableSsl = true,
                };

                var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress("your@email.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }


        #endregion
        #region Resset Password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid request." });

            // إعادة تعيين كلمة المرور باستخدام التوكن
            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!resetPassResult.Succeeded)
            {
                var errors = resetPassResult.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Password reset failed.", errors });
            }

            return Ok(new { message = "Password reset successful." });
        }

        #endregion
        #region Log Out
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto model)
        {
            var result = await _tokenService.RevokeRefreshTokenAsync(model.RefreshToken);
            if (!result)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }
            return Ok(new { message = "Logged out successfully." });
        }

        #endregion
        #region Get All Users
        [Authorize(Roles = "admin")] // فقط الأدمن يقدر يشوف كل المستخدمين
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();

            var userList = new List<UserInfoDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "user"; // لو مفيش رول، افتراضي user

                userList.Add(new UserInfoDto
                {
                    Email = user.Email,
                    Name = user.Name,
                    Role = role
                });
            }

            return Ok(userList);
        }

        #endregion

    }
}
