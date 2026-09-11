using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ORFlow.Infrastructure.Identity;

namespace ORFlow.Api.IntegrationTests.Authentication;

public static class AuthenticationHelper
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        CustomWebApplicationFactory factory,
        string email,
        string role)
    {
        HttpClient client = factory.CreateClient();

        var credentials = new
        {
            Email = email,
            Password = "Test123!"
        };

        HttpResponseMessage registerResponse =
            await client.PostAsJsonAsync(
                "/auth/register",
                credentials);

        registerResponse.EnsureSuccessStatusCode();

        using (IServiceScope scope =
            factory.Services.CreateScope())
        {
            UserManager<ApplicationUser> userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user =
                await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                throw new InvalidOperationException(
                    $"Could not find test user '{email}'.");
            }

            IdentityResult roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not add test user '{email}' to role '{role}'.");
            }
        }

        HttpResponseMessage loginResponse =
            await client.PostAsJsonAsync(
                "/auth/login?useCookies=true",
                credentials);

        loginResponse.EnsureSuccessStatusCode();

        return client;
    }
}
