using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ORFlow.Api.IntegrationTests.Authentication;
using ORFlow.Infrastructure.Identity;

namespace ORFlow.Api.IntegrationTests.SurgeryRequests;

public class SurgeryRequestWorkflowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SurgeryRequestWorkflowTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SurgeryRequest_CanMoveThroughCompleteWorkflow()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        HttpClient schedulerClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"scheduler-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Scheduler);

        var command = new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = Guid.NewGuid(),
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "Integration Test Procedure",
            RequestedStartTime =
                new DateTimeOffset(2026, 10, 1, 14, 0, 0, TimeSpan.Zero),
            RequestedEndTime =
                new DateTimeOffset(2026, 10, 1, 16, 0, 0, TimeSpan.Zero)
        };

        HttpResponseMessage createResponse =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                command);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        JsonElement created =
            await createResponse.Content.ReadFromJsonAsync<JsonElement>();

        Guid surgeryRequestId =
            created.GetProperty("surgeryRequestId").GetGuid();

        Assert.Equal(
            0,
            created.GetProperty("requestStatus").GetInt32());

        Assert.Equal(
            command.RequestedStartTime,
            created.GetProperty("requestedStartTime").GetDateTimeOffset());

        Assert.Equal(
            command.RequestedEndTime,
            created.GetProperty("requestedEndTime").GetDateTimeOffset());

        Assert.False(
            created.TryGetProperty("requestedTime", out _));

        HttpResponseMessage approveResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            approveResponse.StatusCode);

        JsonElement approved =
            await approveResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            1,
            approved.GetProperty("requestStatus").GetInt32());

        HttpResponseMessage scheduleResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/schedule",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            scheduleResponse.StatusCode);

        JsonElement scheduled =
            await scheduleResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            2,
            scheduled.GetProperty("requestStatus").GetInt32());

        HttpResponseMessage completeResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/complete",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            completeResponse.StatusCode);

        JsonElement completed =
            await completeResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            3,
            completed.GetProperty("requestStatus").GetInt32());

        HttpResponseMessage archiveResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/archive",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            archiveResponse.StatusCode);

        JsonElement archived =
            await archiveResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            4,
            archived.GetProperty("requestStatus").GetInt32());

        HttpResponseMessage getResponse =
            await schedulerClient.GetAsync(
                $"/surgery-requests/{surgeryRequestId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        JsonElement retrieved =
            await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            4,
            retrieved.GetProperty("requestStatus").GetInt32());
    }

    [Fact]
    public async Task Schedule_WhenOverlappingRequestUsesSameSurgeon_ReturnsConflict()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        HttpClient schedulerClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"scheduler-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Scheduler);

        Guid surgeonId = Guid.NewGuid();

        DateTimeOffset startTime =
            new DateTimeOffset(
                2026, 10, 2, 14, 0, 0, TimeSpan.Zero);

        var firstCommand = new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = surgeonId,
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "First Procedure",
            RequestedStartTime = startTime,
            RequestedEndTime = startTime.AddHours(2)
        };

        HttpResponseMessage firstCreateResponse =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                firstCommand);

        Assert.Equal(
            HttpStatusCode.Created,
            firstCreateResponse.StatusCode);

        JsonElement firstCreated =
            await firstCreateResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        Guid firstRequestId =
            firstCreated.GetProperty("surgeryRequestId").GetGuid();

        HttpResponseMessage firstApproveResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{firstRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            firstApproveResponse.StatusCode);

        HttpResponseMessage firstScheduleResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{firstRequestId}/schedule",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            firstScheduleResponse.StatusCode);

        var secondCommand = new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = surgeonId,
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "Second Procedure",
            RequestedStartTime = startTime.AddHours(1),
            RequestedEndTime = startTime.AddHours(3)
        };

        HttpResponseMessage secondCreateResponse =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                secondCommand);

        Assert.Equal(
            HttpStatusCode.Created,
            secondCreateResponse.StatusCode);

        JsonElement secondCreated =
            await secondCreateResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        Guid secondRequestId =
            secondCreated.GetProperty("surgeryRequestId").GetGuid();

        HttpResponseMessage secondApproveResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{secondRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            secondApproveResponse.StatusCode);

        HttpResponseMessage secondScheduleResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{secondRequestId}/schedule",
                null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondScheduleResponse.StatusCode);

        JsonElement conflict =
            await secondScheduleResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Scheduling conflict detected.",
            conflict.GetProperty("message").GetString());

        Assert.Equal(
            1,
            conflict
                .GetProperty("surgeryRequest")
                .GetProperty("requestStatus")
                .GetInt32());

        HttpResponseMessage getResponse =
            await schedulerClient.GetAsync(
                $"/surgery-requests/{secondRequestId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        JsonElement retrieved =
            await getResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            1,
            retrieved.GetProperty("requestStatus").GetInt32());
    }

    [Fact]
    public async Task Create_WithInvalidProcedureName_ReturnsBadRequest()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        var request = new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = Guid.NewGuid(),
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "",
            RequestedStartTime =
                new DateTimeOffset(2026, 10, 3, 14, 0, 0, TimeSpan.Zero),
            RequestedEndTime =
                new DateTimeOffset(2026, 10, 3, 16, 0, 0, TimeSpan.Zero)
        };

        HttpResponseMessage response =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        JsonElement problem =
    await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Invalid request",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            400,
            problem.GetProperty("status").GetInt32());

        Assert.Contains(
            "Procedure name cannot be null or empty.",
            problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Create_WhenEndTimeIsNotAfterStartTime_ReturnsBadRequest()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        DateTimeOffset startTime =
            new DateTimeOffset(2026, 10, 3, 14, 0, 0, TimeSpan.Zero);

        var request = new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = Guid.NewGuid(),
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "Invalid Time Procedure",
            RequestedStartTime = startTime,
            RequestedEndTime = startTime
        };

        HttpResponseMessage response =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        JsonElement problem =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Invalid request",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            400,
            problem.GetProperty("status").GetInt32());

        Assert.Contains(
            "End time must be later than start time.",
            problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Complete_WhenRequestIsNotScheduled_ReturnsConflict()
    {
        HttpClient surgeonClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"surgeon-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Surgeon);

        HttpClient schedulerClient =
            await AuthenticationHelper.CreateAuthenticatedClientAsync(
                _factory,
                $"scheduler-{Guid.NewGuid()}@orflow.test",
                ApplicationRoles.Scheduler);

        var request = new
        {
            PatientId = Guid.NewGuid(),
            SurgeonId = Guid.NewGuid(),
            OperatingRoomId = Guid.NewGuid(),
            ProcedureName = "Invalid Transition Procedure",
            RequestedStartTime =
                new DateTimeOffset(2026, 10, 4, 14, 0, 0, TimeSpan.Zero),
            RequestedEndTime =
                new DateTimeOffset(2026, 10, 4, 16, 0, 0, TimeSpan.Zero)
        };

        HttpResponseMessage createResponse =
            await surgeonClient.PostAsJsonAsync(
                "/surgery-requests",
                request);

        JsonElement created =
            await createResponse.Content.ReadFromJsonAsync<JsonElement>();

        Guid surgeryRequestId =
            created.GetProperty("surgeryRequestId").GetGuid();

        HttpResponseMessage approveResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            approveResponse.StatusCode);

        HttpResponseMessage completeResponse =
            await schedulerClient.PostAsync(
                $"/surgery-requests/{surgeryRequestId}/complete",
                null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            completeResponse.StatusCode);

        JsonElement problem =
            await completeResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Invalid operation",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            409,
            problem.GetProperty("status").GetInt32());

        Assert.Contains(
            "Only scheduled surgery requests can be completed.",
            problem.GetProperty("detail").GetString());
    }

}
