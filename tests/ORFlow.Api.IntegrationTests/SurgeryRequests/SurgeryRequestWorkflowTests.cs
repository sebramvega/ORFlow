using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ORFlow.Api.IntegrationTests.SurgeryRequests;

public class SurgeryRequestWorkflowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SurgeryRequestWorkflowTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SurgeryRequest_CanMoveThroughCompleteWorkflow()
    {
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
            await _client.PostAsJsonAsync(
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

        HttpResponseMessage approveResponse =
            await _client.PostAsync(
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
            await _client.PostAsync(
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
            await _client.PostAsync(
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
            await _client.PostAsync(
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
            await _client.GetAsync(
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
            await _client.PostAsJsonAsync(
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
            await _client.PostAsync(
                $"/surgery-requests/{firstRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            firstApproveResponse.StatusCode);

        HttpResponseMessage firstScheduleResponse =
            await _client.PostAsync(
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
            await _client.PostAsJsonAsync(
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
            await _client.PostAsync(
                $"/surgery-requests/{secondRequestId}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            secondApproveResponse.StatusCode);

        HttpResponseMessage secondScheduleResponse =
            await _client.PostAsync(
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
            await _client.GetAsync(
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

}
