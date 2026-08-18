using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Application.SurgeryRequests.Create;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.Tests.SurgeryRequests.Create;

public class CreateSurgeryRequestHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesAndSavesSurgeryRequest()
    {
        // Arrange
        FakeSurgeryRequestRepository repository = new FakeSurgeryRequestRepository();
        CreateSurgeryRequestHandler handler = new CreateSurgeryRequestHandler(repository);

        Guid patientId = Guid.NewGuid();
        Guid surgeonId = Guid.NewGuid();
        Guid operatingRoomId = Guid.NewGuid();

        DateTimeOffset startTime = DateTimeOffset.UtcNow.AddHours(1);
        DateTimeOffset endTime = startTime.AddHours(2);

        CreateSurgeryRequestCommand command = new CreateSurgeryRequestCommand(
            patientId,
            surgeonId,
            operatingRoomId,
            "Appendectomy",
            startTime,
            endTime);

        // Act
        SurgeryRequest result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(patientId, result.PatientId);
        Assert.Equal(surgeonId, result.SurgeonId);
        Assert.Equal(operatingRoomId, result.OperatingRoomId);
        Assert.Equal("Appendectomy", result.ProcedureName);
        Assert.Equal(startTime, result.RequestedTime.Start);
        Assert.Equal(endTime, result.RequestedTime.End);

        Assert.Same(result, repository.AddedSurgeryRequest);
    }

    private sealed class FakeSurgeryRequestRepository
        : ISurgeryRequestRepository
    {
        public SurgeryRequest? AddedSurgeryRequest { get; private set; }

        public Task AddAsync(SurgeryRequest surgeryRequest)
        {
            AddedSurgeryRequest = surgeryRequest;

            return Task.CompletedTask;
        }
    }
}