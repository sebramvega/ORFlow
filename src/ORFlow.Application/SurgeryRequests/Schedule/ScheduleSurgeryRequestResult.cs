using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Schedule;

public sealed record ScheduleSurgeryRequestResult(
    SurgeryRequest? SurgeryRequest,
    bool HasConflict);