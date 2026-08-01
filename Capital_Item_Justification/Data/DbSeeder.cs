using Capital_Item_Justification.Models;
using Microsoft.AspNetCore.Identity;

namespace Capital_Item_Justification.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "HOD",
                "IT",
                "BME",
                "Admin",
                "Purchage committee",
                "COO",
                "Finance",
                "MD",
                "CEO",
                "Purchase"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task SeedAdmin(UserManager<ApplicationUser> userManager)
        {
            var email = "admin@cij.com";

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, "Admin@123");

                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}

