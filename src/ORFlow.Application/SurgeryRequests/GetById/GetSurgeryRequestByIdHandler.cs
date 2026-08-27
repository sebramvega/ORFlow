using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.GetById;

public sealed class GetSurgeryRequestByIdHandler
{
    private readonly ISurgeryRequestRepository _surgeryRequestRepository;

    public GetSurgeryRequestByIdHandler(
        ISurgeryRequestRepository surgeryRequestRepository)
    {
        _surgeryRequestRepository = surgeryRequestRepository;
    }

    public async Task<SurgeryRequest?> HandleAsync(Guid surgeryRequestId)
    {
        return await _surgeryRequestRepository.GetSurgeryRequestByIdAsync(
            surgeryRequestId);
    }
}