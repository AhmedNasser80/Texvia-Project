using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Texvia.Domain.Conctracts;
using Texvia.Domain.Models;
using Texvia.Persistence.Contexts;
using Texvia.Shared.Dtos;

namespace Texvia.Persistence.Repositories
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TexviaDBContext _context;

        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager, TexviaDBContext context)
        {
            _config = config;
            _userManager = userManager;
            _context = context;
        }

        public async Task<TokenResponse> GenerateTokensAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // صلاحية الـ Access Token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();

            // حفظ الـ Refresh Token في قاعدة البيانات مع تاريخ انتهاء صلاحية 7 أيام (مثال)
            var userRefreshToken = new UserRefreshToken
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.UserRefreshTokens.Add(userRefreshToken);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            // ✅ 1. هل التوكن موجود في قاعدة البيانات؟
            var existingToken = await _context.UserRefreshTokens
                .FirstOrDefaultAsync(rt => rt.RefreshToken == refreshToken);

            // ✅ 2. هل التوكن مش منتهي؟ وهل لم يتم رفضه؟ (IsRevoked = false)
            if (existingToken == null || existingToken.ExpiryDate < DateTime.UtcNow || existingToken.IsRevoked)
                return null;

            // ✅ 3. جلب المستخدم المرتبط بالتوكن
            var user = await _userManager.FindByIdAsync(existingToken.UserId);
            if (user == null)
                return null;

            // ✅ 4. استخدام التوكن لمرة واحدة (Token Rotation): حذف القديم
            _context.UserRefreshTokens.Remove(existingToken);
            await _context.SaveChangesAsync();

            // ✅ 5. توليد توكن جديد (Access + Refresh)
            return await GenerateTokensAsync(user);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.Set<UserRefreshToken>()
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);

            if (token == null || token.IsRevoked || token.ExpiryDate <= DateTime.UtcNow)
            {
                // التوكن غير موجود، أو مرفوض، أو منتهي الصلاحية
                return false;
            }

            token.IsRevoked = true;
            _context.Update(token);
            await _context.SaveChangesAsync();

            return true;
        }



    }
    


}
