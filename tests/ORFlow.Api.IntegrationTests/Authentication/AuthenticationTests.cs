using System.Net;
using System.Net.Http.Json;

namespace ORFlow.Api.IntegrationTests.Authentication;

public class AuthenticationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task User_CanRegisterAndLogin()
    {
        var credentials = new
        {
            Email = "scheduler@orflow.test",
            Password = "Test123!"
        };

        HttpResponseMessage registerResponse =
            await _client.PostAsJsonAsync(
                "/auth/register",
                credentials);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        HttpResponseMessage loginResponse =
            await _client.PostAsJsonAsync(
                "/auth/login",
                credentials);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);
    }
}
