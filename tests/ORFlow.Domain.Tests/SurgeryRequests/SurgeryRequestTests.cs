using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Domain.Tests.SurgeryRequests;

public class SurgeryRequestTests
{
    [Fact]
    public void Constructor_ValidRequest_StatusIsSubmitted()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();
    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act
    SurgeryRequest surgeryRequest = new SurgeryRequest(
        patientId,
        surgeonId,
        operatingRoomId,
        "Appendectomy",
        startTime,
        endTime);

    // Assert
    Assert.Equal(RequestStatus.Submitted, surgeryRequest.RequestStatus);
}

[Fact]
public void Constructor_EndTimeBeforeStartTime_ThrowsArgumentException()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();

    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(2);
    DateTimeOffset endTime = startTime.AddHours(-1);

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new SurgeryRequest(
            patientId,
            surgeonId,
            operatingRoomId,
            "Appendectomy",
            startTime,
            endTime));
}

[Fact]
public void Constructor_EndTimeEqualsStartTime_ThrowsArgumentException()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();

    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime;

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new SurgeryRequest(
            patientId,
            surgeonId,
            operatingRoomId,
            "Appendectomy",
            startTime,
            endTime));
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void Constructor_InvalidProcedureName_ThrowsArgumentException(string? procedureName)
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();

    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new SurgeryRequest(
            patientId,
            surgeonId,
            operatingRoomId,
            procedureName!,
            startTime,
            endTime));
}

[Fact]
public void Constructor_EmptyPatientId_ThrowsArgumentException()
{
    // Arrange
    Guid patientId = Guid.Empty;
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();
    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new SurgeryRequest(
            patientId,
            surgeonId,
            operatingRoomId,
            "Appendectomy",
            startTime,
            endTime));
}

[Fact]
public void Constructor_EmptySurgeonId_ThrowsArgumentException()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.Empty;
    Guid operatingRoomId = Guid.NewGuid();
    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new SurgeryRequest(
            patientId,
            surgeonId,
            operatingRoomId,
            "Appendectomy",
            startTime,
            endTime));
}

[Fact]
public void Constructor_EmptyOperatingRoomId_ThrowsArgumentException()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.Empty;
    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new SurgeryRequest(
            patientId,
            surgeonId,
            operatingRoomId,
            "Appendectomy",
            startTime,
            endTime));
}

[Fact]
public void Constructor_ValidRequest_SetsProvidedProperties()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();
    string procedureName = "Appendectomy";
    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act
    SurgeryRequest surgeryRequest = new SurgeryRequest(
        patientId,
        surgeonId,
        operatingRoomId,
        procedureName,
        startTime,
        endTime);

    // Assert
    Assert.Equal(patientId, surgeryRequest.PatientId);
    Assert.Equal(surgeonId, surgeryRequest.SurgeonId);
    Assert.Equal(operatingRoomId, surgeryRequest.OperatingRoomId);
    Assert.Equal(procedureName, surgeryRequest.ProcedureName);
    Assert.Equal(startTime, surgeryRequest.RequestedTime.Start);
    Assert.Equal(endTime, surgeryRequest.RequestedTime.End);
}

[Fact]
public void Constructor_ValidRequest_GeneratesSurgeryRequestId()
{
    // Arrange
    Guid patientId = Guid.NewGuid();
    Guid surgeonId = Guid.NewGuid();
    Guid operatingRoomId = Guid.NewGuid();
    DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset endTime = startTime.AddHours(2);

    // Act
    SurgeryRequest surgeryRequest = new SurgeryRequest(
        patientId,
        surgeonId,
        operatingRoomId,
        "Appendectomy",
        startTime,
        endTime);

    // Assert
    Assert.NotEqual(Guid.Empty, surgeryRequest.SurgeryRequestId);
}

[Fact]
public void Approve_SubmittedRequest_StatusBecomesApproved()
{
    // Arrange
    SurgeryRequest surgeryRequest = CreateValidSurgeryRequest();

    // Act
    surgeryRequest.Approve();

    // Assert
    Assert.Equal(RequestStatus.Approved, surgeryRequest.RequestStatus);
}

[Fact]
public void Approve_AlreadyApprovedRequest_ThrowsInvalidOperationException()
{
    // Arrange
    SurgeryRequest surgeryRequest = CreateValidSurgeryRequest();
    surgeryRequest.Approve();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        surgeryRequest.Approve());
}

[Fact]
public void Lifecycle_ValidTransitions_ReachesArchivedStatus()
{
    // Arrange
    SurgeryRequest surgeryRequest = CreateValidSurgeryRequest();

    // Act & Assert
    surgeryRequest.Approve();
    Assert.Equal(RequestStatus.Approved, surgeryRequest.RequestStatus);

    surgeryRequest.Schedule();
    Assert.Equal(RequestStatus.Scheduled, surgeryRequest.RequestStatus);

    surgeryRequest.Complete();
    Assert.Equal(RequestStatus.Completed, surgeryRequest.RequestStatus);

    surgeryRequest.Archive();
    Assert.Equal(RequestStatus.Archived, surgeryRequest.RequestStatus);
}

[Fact]
public void Schedule_SubmittedRequest_ThrowsInvalidOperationException()
{
    // Arrange
    SurgeryRequest surgeryRequest = CreateValidSurgeryRequest();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        surgeryRequest.Schedule());
}

[Fact]
public void Complete_ApprovedRequest_ThrowsInvalidOperationException()
{
    // Arrange
    SurgeryRequest surgeryRequest = CreateValidSurgeryRequest();
    surgeryRequest.Approve();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        surgeryRequest.Complete());
}

[Fact]
public void Archive_ScheduledRequest_ThrowsInvalidOperationException()
{
    // Arrange
    SurgeryRequest surgeryRequest = CreateValidSurgeryRequest();
    surgeryRequest.Approve();
    surgeryRequest.Schedule();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        surgeryRequest.Archive());
}

[Fact]
public void OverlapsWith_OverlappingRequests_ReturnsTrue()
{
    // Arrange
    DateTimeOffset start = DateTimeOffset.UtcNow;

    SurgeryRequest first = new SurgeryRequest(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Appendectomy",
        start,
        start.AddHours(2));

    SurgeryRequest second = new SurgeryRequest(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Knee Replacement",
        start.AddHours(1),
        start.AddHours(3));

    // Act
    bool result = first.OverlapsWith(second);

    // Assert
    Assert.True(result);
}

[Fact]
public void ConflictsWith_OverlappingSameSurgeon_ReturnsTrue()
{
    // Arrange
    Guid sharedSurgeonId = Guid.NewGuid();
    DateTimeOffset start = DateTimeOffset.UtcNow;

    SurgeryRequest first = CreateValidSurgeryRequest(
        surgeonId: sharedSurgeonId,
        startTime: start,
        endTime: start.AddHours(2));

    SurgeryRequest second = CreateValidSurgeryRequest(
        surgeonId: sharedSurgeonId,
        startTime: start.AddHours(1),
        endTime: start.AddHours(3));

    // Act
    bool result = first.ConflictsWith(second);

    // Assert
    Assert.True(result);
}

[Fact]
public void ConflictsWith_OverlappingSameOperatingRoom_ReturnsTrue()
{
    // Arrange
    Guid sharedRoomId = Guid.NewGuid();
    DateTimeOffset start = DateTimeOffset.UtcNow;

    SurgeryRequest first = CreateValidSurgeryRequest(
        operatingRoomId: sharedRoomId,
        startTime: start,
        endTime: start.AddHours(2));

    SurgeryRequest second = CreateValidSurgeryRequest(
        operatingRoomId: sharedRoomId,
        startTime: start.AddHours(1),
        endTime: start.AddHours(3));

    // Act
    bool result = first.ConflictsWith(second);

    // Assert
    Assert.True(result);
}

[Fact]
public void ConflictsWith_OverlappingDifferentResources_ReturnsFalse()
{
    // Arrange
    DateTimeOffset start = DateTimeOffset.UtcNow;

    SurgeryRequest first = CreateValidSurgeryRequest(
        startTime: start,
        endTime: start.AddHours(2));

    SurgeryRequest second = CreateValidSurgeryRequest(
        startTime: start.AddHours(1),
        endTime: start.AddHours(3));

    // Act
    bool result = first.ConflictsWith(second);

    // Assert
    Assert.False(result);
}

[Fact]
public void ConflictsWith_NonOverlappingSameResources_ReturnsFalse()
{
    // Arrange
    Guid sharedSurgeonId = Guid.NewGuid();
    Guid sharedRoomId = Guid.NewGuid();
    DateTimeOffset start = DateTimeOffset.UtcNow;

    SurgeryRequest first = CreateValidSurgeryRequest(
        surgeonId: sharedSurgeonId,
        operatingRoomId: sharedRoomId,
        startTime: start,
        endTime: start.AddHours(2));

    SurgeryRequest second = CreateValidSurgeryRequest(
        surgeonId: sharedSurgeonId,
        operatingRoomId: sharedRoomId,
        startTime: start.AddHours(3),
        endTime: start.AddHours(5));

    // Act
    bool result = first.ConflictsWith(second);

    // Assert
    Assert.False(result);
}

private static SurgeryRequest CreateValidSurgeryRequest(
    Guid? surgeonId = null,
    Guid? operatingRoomId = null,
    DateTimeOffset? startTime = null,
    DateTimeOffset? endTime = null)
{
    DateTimeOffset start = startTime ?? DateTimeOffset.UtcNow.AddHours(1);
    DateTimeOffset end = endTime ?? start.AddHours(2);

    return new SurgeryRequest(
        Guid.NewGuid(),
        surgeonId ?? Guid.NewGuid(),
        operatingRoomId ?? Guid.NewGuid(),
        "Appendectomy",
        start,
        end);
}

}