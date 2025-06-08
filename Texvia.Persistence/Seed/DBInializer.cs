using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texvia.Domain.Conctracts;
using Texvia.Domain.Models;
using Texvia.Persistence.Contexts;

namespace Texvia.Persistence.Seed
{
    public class DBInializer : IDBInializer
    {
        private readonly TexviaDBContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public DBInializer(TexviaDBContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager
        )

        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task InializeAsync()
        {
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
            }

            string[] roles = new[] { "admin", "user" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        // سجل الخطأ هنا أو تعامل معه حسب النظام
                        throw new Exception($"Failed to create role: {role}");
                    }
                }
            }

            var adminEmail = "admin@texvia.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    Name = "Administrator"  // اسم الادمن
                };

                var result = await _userManager.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    var roleAssignResult = await _userManager.AddToRoleAsync(user, "admin");
                    if (!roleAssignResult.Succeeded)
                    {
                        // سجل الخطأ هنا أو تعامل معه
                        throw new Exception("Failed to assign admin role to the default admin user.");
                    }
                }
                else
                {
                    // سجل الخطأ هنا أو تعامل معه
                    throw new Exception("Failed to create the default admin user.");
                }
            }
        }


    }
}
