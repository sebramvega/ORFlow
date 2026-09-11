using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Schedule;

public sealed class ScheduleSurgeryRequestHandler
{
    private readonly ISurgeryRequestRepository _surgeryRequestRepository;

    public ScheduleSurgeryRequestHandler(
        ISurgeryRequestRepository surgeryRequestRepository)
    {
        _surgeryRequestRepository = surgeryRequestRepository;
    }

    public async Task<ScheduleSurgeryRequestResult> HandleAsync(
        Guid surgeryRequestId)
    {
        SurgeryRequest? surgeryRequest =
            await _surgeryRequestRepository.GetSurgeryRequestByIdAsync(
                surgeryRequestId);

        if (surgeryRequest is null)
        {
            return new ScheduleSurgeryRequestResult(null, false);
        }

        IReadOnlyList<SurgeryRequest> overlappingRequests =
            await _surgeryRequestRepository.GetOverlappingRequestsAsync(
                surgeryRequest.RequestedTime);

        bool hasConflict = overlappingRequests.Any(
            other =>
                other.SurgeryRequestId != surgeryRequest.SurgeryRequestId &&
                surgeryRequest.ConflictsWith(other));

        if (hasConflict)
        {
            return new ScheduleSurgeryRequestResult(
                surgeryRequest,
                true);
        }

        surgeryRequest.Schedule();

        await _surgeryRequestRepository.SaveChangesAsync();

        return new ScheduleSurgeryRequestResult(
            surgeryRequest,
            false);
    }
}