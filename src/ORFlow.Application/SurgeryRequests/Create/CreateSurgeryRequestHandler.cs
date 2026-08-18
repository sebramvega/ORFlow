using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Create;

public sealed class CreateSurgeryRequestHandler
{
    private readonly ISurgeryRequestRepository _surgeryRequestRepository;

    public CreateSurgeryRequestHandler(
        ISurgeryRequestRepository surgeryRequestRepository)
    {
        _surgeryRequestRepository = surgeryRequestRepository;
    }

    public async Task<SurgeryRequest> HandleAsync(
        CreateSurgeryRequestCommand command)
    {
        SurgeryRequest surgeryRequest = new SurgeryRequest(
            command.PatientId,
            command.SurgeonId,
            command.OperatingRoomId,
            command.ProcedureName,
            command.RequestedStartTime,
            command.RequestedEndTime);

        await _surgeryRequestRepository.AddAsync(surgeryRequest);

        return surgeryRequest;
    }
}