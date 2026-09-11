using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ORFlow.Infrastructure.Identity;

namespace ORFlow.Api.IntegrationTests.Authentication;

public class AuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateSurgeryRequest_WhenAnonymous_ReturnsUnauthorized()
    {
        HttpClient client = _factory.CreateClient();

        var command = CreateCommand();

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/surgery-requests",
                command);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateSurgeryRequest_WhenScheduler_ReturnsForbidden()
    {
        HttpClient schedulerClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"scheduler-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Scheduler);

        var command = CreateCommand();

        HttpResponseMessage response =
            await schedulerClient.PostAsJsonAsync(
                "/surgery-requests",
                command);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateSurgeryRequest_WhenSurgeon_ReturnsCreated()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        var command = CreateCommand();

        HttpResponseMessage response =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                command);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task ApproveSurgeryRequest_WhenAnonymous_ReturnsUnauthorized()
    {
        Guid surgeryRequestId =
            await CreateSurgeryRequestAsync();

        HttpClient anonymousClient =
            _factory.CreateClient();

        HttpResponseMessage response =
            await anonymousClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ApproveSurgeryRequest_WhenSurgeon_ReturnsForbidden()
    {
        Guid surgeryRequestId =
            await CreateSurgeryRequestAsync();

        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        HttpResponseMessage response =
            await surgeonClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task ApproveSurgeryRequest_WhenScheduler_ReturnsOk()
    {
        Guid surgeryRequestId =
            await CreateSurgeryRequestAsync();

        HttpClient schedulerClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"scheduler-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Scheduler);

        HttpResponseMessage response =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateSurgeryRequest_WhenAdministrator_ReturnsCreated()
    {
        HttpClient administratorClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"administrator-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Administrator);

        var command = CreateCommand();

        HttpResponseMessage response =
            await administratorClient.PostAsJsonAsync(
                "/surgery-requests",
                command);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task ApproveSurgeryRequest_WhenAdministrator_ReturnsOk()
    {
        Guid surgeryRequestId =
            await CreateSurgeryRequestAsync();

        HttpClient administratorClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"administrator-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Administrator);

        HttpResponseMessage response =
            await administratorClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    private async Task<Guid> CreateSurgeryRequestAsync()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        HttpResponseMessage response =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                CreateCommand());

        response.EnsureSuccessStatusCode();

        JsonElement created =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        return created
            .GetProperty("surgeryRequestId")
            .GetGuid();
    }

    private static object CreateCommand()
    {
        return new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = Guid.NewGuid(),
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "Authorization Test Procedure",
            RequestedStartTime =
                new DateTimeOffset(
                    2026, 11, 1, 14, 0, 0, TimeSpan.Zero),
            RequestedEndTime =
                new DateTimeOffset(
                    2026, 11, 1, 16, 0, 0, TimeSpan.Zero)
        };
    }
}
