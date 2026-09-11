using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Application.SurgeryRequests.GetById;
using ORFlow.Domain.SurgeryRequests;
using ORFlow.Domain.Scheduling;

namespace ORFlow.Application.Tests.SurgeryRequests.GetById;

public class GetSurgeryRequestByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenSurgeryRequestExists_ReturnsSurgeryRequest()
    {
        // Arrange
        SurgeryRequest surgeryRequest = new SurgeryRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Appendectomy",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(3));

        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(surgeryRequest);

        GetSurgeryRequestByIdHandler handler =
            new GetSurgeryRequestByIdHandler(repository);

        // Act
        SurgeryRequest? result =
            await handler.HandleAsync(surgeryRequest.SurgeryRequestId);

        // Assert
        Assert.Same(surgeryRequest, result);
    }

    [Fact]
    public async Task HandleAsync_WhenSurgeryRequestDoesNotExist_ReturnsNull()
    {
        // Arrange
        FakeSurgeryRequestRepository repository =
            new FakeSurgeryRequestRepository(null);

        GetSurgeryRequestByIdHandler handler =
            new GetSurgeryRequestByIdHandler(repository);

        // Act
        SurgeryRequest? result =
            await handler.HandleAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    private sealed class FakeSurgeryRequestRepository
        : ISurgeryRequestRepository
    {
        private readonly SurgeryRequest? _surgeryRequest;

        public FakeSurgeryRequestRepository(SurgeryRequest? surgeryRequest)
        {
            _surgeryRequest = surgeryRequest;
        }

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
            return Task.CompletedTask;
        }
    }
}