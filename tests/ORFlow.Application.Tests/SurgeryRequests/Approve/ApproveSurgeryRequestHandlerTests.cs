using ORFlow.Application.SurgeryRequests.Approve;
using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.Tests.SurgeryRequests.Approve;

public class ApproveSurgeryRequestHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenSurgeryRequestExists_ApprovesAndSaves()
    {
        SurgeryRequest surgeryRequest = CreateSurgeryRequest();

        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(surgeryRequest);

        ApproveSurgeryRequestHandler handler =
            new ApproveSurgeryRequestHandler(repository);

        SurgeryRequest? result =
            await handler.HandleAsync(surgeryRequest.SurgeryRequestId);

        Assert.NotNull(result);
        Assert.Equal(RequestStatus.Approved, result.RequestStatus);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenSurgeryRequestDoesNotExist_ReturnsNull()
    {
        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(null);

        ApproveSurgeryRequestHandler handler =
            new ApproveSurgeryRequestHandler(repository);

        SurgeryRequest? result =
            await handler.HandleAsync(Guid.NewGuid());

        Assert.Null(result);
        Assert.False(repository.SaveChangesCalled);
    }

    private static SurgeryRequest CreateSurgeryRequest()
    {
        DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);

        return new SurgeryRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Appendectomy",
            startTime,
            startTime.AddHours(2));
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

        public Task SaveChangesAsync()
        {
            SaveChangesCalled = true;

            return Task.CompletedTask;
        }
    }
}