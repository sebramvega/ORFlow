using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Approve;

public sealed class ApproveSurgeryRequestHandler
{
    private readonly ISurgeryRequestRepository _surgeryRequestRepository;

    public ApproveSurgeryRequestHandler(
        ISurgeryRequestRepository surgeryRequestRepository)
    {
        _surgeryRequestRepository = surgeryRequestRepository;
    }

    public async Task<SurgeryRequest?> HandleAsync(Guid surgeryRequestId)
    {
        SurgeryRequest? surgeryRequest =
            await _surgeryRequestRepository.GetSurgeryRequestByIdAsync(
                surgeryRequestId);

        if (surgeryRequest is null)
        {
            return null;
        }

        surgeryRequest.Approve();

        await _surgeryRequestRepository.SaveChangesAsync();

        return surgeryRequest;
    }
}