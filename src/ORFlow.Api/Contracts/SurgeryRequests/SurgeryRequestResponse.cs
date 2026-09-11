using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Api.Contracts.SurgeryRequests;

public sealed record SurgeryRequestResponse(
    Guid SurgeryRequestId,
    Guid PatientId,
    Guid SurgeonId,
    Guid OperatingRoomId,
    string ProcedureName,
    DateTimeOffset RequestedStartTime,
    DateTimeOffset RequestedEndTime,
    RequestStatus RequestStatus);