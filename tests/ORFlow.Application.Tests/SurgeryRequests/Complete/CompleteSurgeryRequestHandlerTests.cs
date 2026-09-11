using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Application.SurgeryRequests.Complete;
using ORFlow.Domain.Scheduling;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.Tests.SurgeryRequests.Complete;

public class CompleteSurgeryRequestHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenScheduledSurgeryRequestExists_CompletesAndSaves()
    {
        SurgeryRequest surgeryRequest = CreateScheduledSurgeryRequest();

        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(surgeryRequest);

        CompleteSurgeryRequestHandler handler =
            new CompleteSurgeryRequestHandler(repository);

        SurgeryRequest? result =
            await handler.HandleAsync(surgeryRequest.SurgeryRequestId);

        Assert.NotNull(result);
        Assert.Equal(RequestStatus.Completed, result.RequestStatus);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenSurgeryRequestDoesNotExist_ReturnsNull()
    {
        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(null);

        CompleteSurgeryRequestHandler handler =
            new CompleteSurgeryRequestHandler(repository);

        SurgeryRequest? result =
            await handler.HandleAsync(Guid.NewGuid());

        Assert.Null(result);
        Assert.False(repository.SaveChangesCalled);
    }

    private static SurgeryRequest CreateScheduledSurgeryRequest()
    {
        DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);

        SurgeryRequest surgeryRequest = new SurgeryRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Appendectomy",
            startTime,
            startTime.AddHours(2));

        surgeryRequest.Approve();
        surgeryRequest.Schedule();

        return surgeryRequest;
    }

    private sealed class FakeSurgeryRequestRepository
        : ISurgeryRequestRepository
    {
        private readonly SurgeryRequest? _surgeryRequest;

        public FakeSurgeryRequestRepository(SurgeryRequest? surgeryRequest)
        {
            _surgeryRequest = surgeryRequest;
        }

        public bool SaveChangesCalled { get; private set; }

        public Task AddAsync(SurgeryRequest surgeryRequest)
        {
            return Task.CompletedTask;
        }

        public Task<SurgeryRequest?> GetSurgeryRequestByIdAsync(
            Guid surgeryRequestId)
        {
            if (_surgeryRequest?.SurgeryRequestId == surgeryRequestId)
            {
                return Task.FromResult<SurgeryRequest?>(_surgeryRequest);
            }

            return Task.FromResult<SurgeryRequest?>(null);
        }

        public Task<IReadOnlyList<SurgeryRequest>> GetOverlappingRequestsAsync(
            TimeRange requestedTime)
        {
            return Task.FromResult<IReadOnlyList<SurgeryRequest>>(
                Array.Empty<SurgeryRequest>());
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCalled = true;

            return Task.CompletedTask;
        }
    }
}
