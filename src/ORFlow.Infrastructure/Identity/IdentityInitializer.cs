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

    public static async Task InitializeDevelopmentUsersAsync(
        UserManager<ApplicationUser> userManager)
    {
        await EnsureDevelopmentUserAsync(
            userManager,
            "surgeon@orflow.local",
            "Surgeon123!",
            ApplicationRoles.Surgeon);

        await EnsureDevelopmentUserAsync(
            userManager,
            "scheduler@orflow.local",
            "Scheduler123!",
            ApplicationRoles.Scheduler);
    }

    private static async Task EnsureDevelopmentUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        ApplicationUser? user =
            await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            IdentityResult createResult =
                await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create development user '{email}'.");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            IdentityResult roleResult =
                await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add development user '{email}' to role '{role}'.");
            }
        }
    }
}
