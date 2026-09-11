using ORFlow.Domain.Scheduling;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Common;

public interface ISurgeryRequestRepository
{
    Task AddAsync(SurgeryRequest surgeryRequest);

    Task<SurgeryRequest?> GetSurgeryRequestByIdAsync(Guid surgeryRequestId);

    Task<IReadOnlyList<SurgeryRequest>> GetOverlappingRequestsAsync(
        TimeRange requestedTime);

    Task SaveChangesAsync();
}