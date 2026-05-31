using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MVCProject.Models;

namespace MVCProject.Services;

public static class IdentitySeed
{
    public const string AdminRole = "Admin";
    public const string MemberRole = "Member";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminSettings = serviceProvider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;

        foreach (var roleName in new[] { AdminRole, MemberRole })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(adminSettings.Email);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminSettings.UserName,
                Email = adminSettings.Email,
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminSettings.Password);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
    }
}
