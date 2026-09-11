using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Application.SurgeryRequests.Schedule;
using ORFlow.Domain.Scheduling;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.Tests.SurgeryRequests.Schedule;

public class ScheduleSurgeryRequestHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRequestDoesNotExist_ReturnsNotFoundResult()
    {
        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(null);

        ScheduleSurgeryRequestHandler handler =
            new ScheduleSurgeryRequestHandler(repository);

        ScheduleSurgeryRequestResult result =
            await handler.HandleAsync(Guid.NewGuid());

        Assert.Null(result.SurgeryRequest);
        Assert.False(result.HasConflict);
        Assert.False(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenNoConflict_SchedulesAndSaves()
    {
        SurgeryRequest surgeryRequest = CreateApprovedSurgeryRequest();

        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(surgeryRequest);

        ScheduleSurgeryRequestHandler handler =
            new ScheduleSurgeryRequestHandler(repository);

        ScheduleSurgeryRequestResult result =
            await handler.HandleAsync(surgeryRequest.SurgeryRequestId);

        Assert.Same(surgeryRequest, result.SurgeryRequest);
        Assert.False(result.HasConflict);
        Assert.Equal(RequestStatus.Scheduled, surgeryRequest.RequestStatus);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenSurgeonConflictExists_DoesNotScheduleOrSave()
    {
        Guid surgeonId = Guid.NewGuid();

        SurgeryRequest surgeryRequest =
            CreateApprovedSurgeryRequest(surgeonId: surgeonId);

        SurgeryRequest conflictingRequest =
            CreateApprovedSurgeryRequest(surgeonId: surgeonId);

        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(
                surgeryRequest,
                conflictingRequest);

        ScheduleSurgeryRequestHandler handler =
            new ScheduleSurgeryRequestHandler(repository);

        ScheduleSurgeryRequestResult result =
            await handler.HandleAsync(surgeryRequest.SurgeryRequestId);

        Assert.Same(surgeryRequest, result.SurgeryRequest);
        Assert.True(result.HasConflict);
        Assert.Equal(RequestStatus.Approved, surgeryRequest.RequestStatus);
        Assert.False(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenOperatingRoomConflictExists_DoesNotScheduleOrSave()
    {
        Guid operatingRoomId = Guid.NewGuid();

        SurgeryRequest surgeryRequest =
            CreateApprovedSurgeryRequest(operatingRoomId: operatingRoomId);

        SurgeryRequest conflictingRequest =
            CreateApprovedSurgeryRequest(operatingRoomId: operatingRoomId);

        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(
                surgeryRequest,
                conflictingRequest);

        ScheduleSurgeryRequestHandler handler =
            new ScheduleSurgeryRequestHandler(repository);

        ScheduleSurgeryRequestResult result =
            await handler.HandleAsync(surgeryRequest.SurgeryRequestId);

        Assert.True(result.HasConflict);
        Assert.Equal(RequestStatus.Approved, surgeryRequest.RequestStatus);
        Assert.False(repository.SaveChangesCalled);
    }

    private static SurgeryRequest CreateApprovedSurgeryRequest(
        Guid? surgeonId = null,
        Guid? operatingRoomId = null)
    {
        DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);

        SurgeryRequest surgeryRequest = new SurgeryRequest(
            Guid.NewGuid(),
            surgeonId ?? Guid.NewGuid(),
            operatingRoomId ?? Guid.NewGuid(),
            "Appendectomy",
            startTime,
            startTime.AddHours(2));

        surgeryRequest.Approve();

        return surgeryRequest;
    }

    private sealed class FakeSurgeryRequestRepository
        : ISurgeryRequestRepository
    {
        private readonly SurgeryRequest? _surgeryRequest;
        private readonly IReadOnlyList<SurgeryRequest> _overlappingRequests;

        public FakeSurgeryRequestRepository(
            SurgeryRequest? surgeryRequest,
            params SurgeryRequest[] overlappingRequests)
        {
            _surgeryRequest = surgeryRequest;
            _overlappingRequests = overlappingRequests;
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
            return Task.FromResult(_overlappingRequests);
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCalled = true;

            return Task.CompletedTask;
        }
    }
}