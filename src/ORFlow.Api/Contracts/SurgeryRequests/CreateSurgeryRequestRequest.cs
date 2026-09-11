namespace ORFlow.Api.Contracts.SurgeryRequests;

public sealed record CreateSurgeryRequestRequest(
    Guid PatientId,
    Guid SurgeonId,
    Guid OperatingRoomId,
    string ProcedureName,
    DateTimeOffset RequestedStartTime,
    DateTimeOffset RequestedEndTime);