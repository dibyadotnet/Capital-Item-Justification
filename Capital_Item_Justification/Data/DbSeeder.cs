using Capital_Item_Justification.Models;
using Microsoft.AspNetCore.Identity;

namespace Capital_Item_Justification.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles =
            {
            "Admin"
            };

            foreach (var roleName in roles)
            {
                var existingRole = await roleManager.FindByNameAsync(roleName);

                if (existingRole == null)
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        IsActive = true,
                        CreatedBy = "System",
                        CreatedOn = DateTime.UtcNow
                    };

                    var result = await roleManager.CreateAsync(role);

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            result.Errors.Select(e => e.Description));

                        throw new Exception(
                            $"Failed to create role '{roleName}': {errors}");
                    }
                }
                else if (!existingRole.IsActive)
                {
                    // Reactivate role if it already exists but was soft deleted
                    existingRole.IsActive = true;
                    existingRole.ModifiedBy = "System";
                    existingRole.ModifiedOn = DateTime.UtcNow;

                    var result = await roleManager.UpdateAsync(existingRole);

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            result.Errors.Select(e => e.Description));

                        throw new Exception(
                            $"Failed to activate role '{roleName}': {errors}");
                    }
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

