using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Archive;

public sealed class ArchiveSurgeryRequestHandler
{
    private readonly ISurgeryRequestRepository _surgeryRequestRepository;

    public ArchiveSurgeryRequestHandler(
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

        surgeryRequest.Archive();

        await _surgeryRequestRepository.SaveChangesAsync();

        return surgeryRequest;
    }
}