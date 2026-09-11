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
}
