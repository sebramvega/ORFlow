using Microsoft.AspNetCore.Identity;

namespace ORFlow.Infrastructure.Identity;

public static class IdentityInitializer
{
    public static async Task InitializeRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        [
            ApplicationRoles.Surgeon,
            ApplicationRoles.Scheduler,
            ApplicationRoles.Administrator
        ];

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                IdentityResult result = await roleManager.CreateAsync(
                    new IdentityRole<Guid>(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create Identity role '{role}'.");
                }
            }
        }
    }
}
