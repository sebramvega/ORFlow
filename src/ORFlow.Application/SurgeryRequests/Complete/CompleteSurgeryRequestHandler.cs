using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Complete;

public sealed class CompleteSurgeryRequestHandler
{
    private readonly ISurgeryRequestRepository _surgeryRequestRepository;

    public CompleteSurgeryRequestHandler(
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

        surgeryRequest.Complete();

        await _surgeryRequestRepository.SaveChangesAsync();

        return surgeryRequest;
    }
}
