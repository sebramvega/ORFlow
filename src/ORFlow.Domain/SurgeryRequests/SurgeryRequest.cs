using ORFlow.Domain.Scheduling;
namespace ORFlow.Domain.SurgeryRequests;

public class SurgeryRequest
{

    private SurgeryRequest()
    {
    }
    public SurgeryRequest(
        Guid patientId,
        Guid surgeonId,
        Guid operatingRoomId,
        string procedureName,
        DateTimeOffset requestedStartTime,
        DateTimeOffset requestedEndTime)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient ID cannot be empty.", nameof(patientId));
        }

        if (surgeonId == Guid.Empty)
        {
            throw new ArgumentException("Surgeon ID cannot be empty.", nameof(surgeonId));
        }

        if (operatingRoomId == Guid.Empty)
        {
            throw new ArgumentException("Operating room ID cannot be empty.", nameof(operatingRoomId));
        }

        if (string.IsNullOrWhiteSpace(procedureName))
        {
            throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
        }

        SurgeryRequestId = Guid.NewGuid();
        RequestStatus = RequestStatus.Submitted;

        PatientId = patientId;
        SurgeonId = surgeonId;
        OperatingRoomId = operatingRoomId;
        ProcedureName = procedureName;
        RequestedTime = new TimeRange(
            requestedStartTime,
            requestedEndTime
        );
    }

    public Guid SurgeryRequestId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid SurgeonId { get; private set; }
    public Guid OperatingRoomId { get; private set; }
    public string ProcedureName { get; private set; } = null!;
    public TimeRange RequestedTime { get; private set; } = null!;
    public RequestStatus RequestStatus { get; private set; }

    public void Approve()
    {
        if (RequestStatus != RequestStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted surgery requests can be approved.");
        }

        RequestStatus = RequestStatus.Approved;
    }
    public bool OverlapsWith(SurgeryRequest other)
    {
        return RequestedTime.OverlapsWith(other.RequestedTime);
    }

    public bool ConflictsWith(SurgeryRequest other)
    {
        bool sharesSurgeon = SurgeonId == other.SurgeonId;
        bool sharesOperatingRoom = OperatingRoomId == other.OperatingRoomId;

        return OverlapsWith(other)
            && (sharesSurgeon || sharesOperatingRoom);
    }

    public void Schedule()
    {
        if (RequestStatus != RequestStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved surgery requests can be scheduled.");
        }

        RequestStatus = RequestStatus.Scheduled;
    }

    public void Complete()
    {
        if (RequestStatus != RequestStatus.Scheduled)
        {
            throw new InvalidOperationException(
                "Only scheduled surgery requests can be completed.");
        }

        RequestStatus = RequestStatus.Completed;
    }

    public void Archive()
    {
        if (RequestStatus != RequestStatus.Completed)
        {
            throw new InvalidOperationException(
                "Only completed surgery requests can be archived.");
        }

        RequestStatus = RequestStatus.Archived;
    }
}