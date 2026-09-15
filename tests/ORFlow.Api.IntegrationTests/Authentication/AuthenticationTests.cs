using System.Net;
using System.Net.Http.Json;
using ORFlow.Api.Contracts.Authentication;
using ORFlow.Infrastructure.Identity;

namespace ORFlow.Api.IntegrationTests.Authentication;

public class AuthenticationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthenticationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_CanRegisterAndLogin()
    {
        HttpClient client = _factory.CreateClient();

        var credentials = new
        {
            Email = $"user-{Guid.NewGuid()}@orflow.test",
            Password = "Test123!"
        };

        HttpResponseMessage registerResponse =
            await client.PostAsJsonAsync(
                "/auth/register",
                credentials);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        HttpResponseMessage loginResponse =
            await client.PostAsJsonAsync(
                "/auth/login",
                credentials);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WhenAnonymous_ReturnsUnauthorized()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response =
            await client.GetAsync("/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WhenAuthenticated_ReturnsEmailAndRoles()
    {
        string email =
            $"surgeon-{Guid.NewGuid()}@orflow.test";

        HttpClient client =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                email,
                ApplicationRoles.Surgeon);

        HttpResponseMessage response =
            await client.GetAsync("/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        CurrentUserResponse? currentUser =
            await response.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        Assert.NotNull(currentUser);
        Assert.Equal(email, currentUser.Email);
        Assert.Contains(
            ApplicationRoles.Surgeon,
            currentUser.Roles);
    }
    [Fact]
    public async Task Logout_WhenAuthenticated_RemovesAuthentication()
    {
        HttpClient client =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        HttpResponseMessage beforeLogout =
            await client.GetAsync("/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            beforeLogout.StatusCode);

        HttpResponseMessage logoutResponse =
            await client.PostAsync(
                "/auth/logout",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            logoutResponse.StatusCode);

        HttpResponseMessage afterLogout =
            await client.GetAsync("/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            afterLogout.StatusCode);
    }
}

