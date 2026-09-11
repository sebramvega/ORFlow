using System.Net;

namespace ORFlow.Api.IntegrationTests.Cors;

public class CorsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CorsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Preflight_FromFrontendOrigin_ReturnsExpectedCorsHeaders()
    {
        HttpClient client = _factory.CreateClient();

        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Options,
                "/surgery-requests");

        request.Headers.Add(
            "Origin",
            "http://localhost:5173");

        request.Headers.Add(
            "Access-Control-Request-Method",
            "POST");

        HttpResponseMessage response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        Assert.Equal(
            "http://localhost:5173",
            response.Headers
                .GetValues("Access-Control-Allow-Origin")
                .Single());

        Assert.Equal(
            "true",
            response.Headers
                .GetValues("Access-Control-Allow-Credentials")
                .Single());
    }
}
