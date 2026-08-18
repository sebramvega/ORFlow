namespace ORFlow.Application.SurgeryRequests.Create;

public sealed record CreateSurgeryRequestCommand(
    Guid PatientId,
    Guid SurgeonId,
    Guid OperatingRoomId,
    string ProcedureName,
    DateTimeOffset RequestedStartTime,
    DateTimeOffset RequestedEndTime);