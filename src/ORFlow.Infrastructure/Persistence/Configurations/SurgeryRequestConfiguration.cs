using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Infrastructure.Persistence.Configurations;

public sealed class SurgeryRequestConfiguration
    : IEntityTypeConfiguration<SurgeryRequest>
{
    public void Configure(EntityTypeBuilder<SurgeryRequest> builder)
    {
        builder.HasKey(surgeryRequest => surgeryRequest.SurgeryRequestId);
        
        builder.Property(surgeryRequest => surgeryRequest.PatientId)
            .IsRequired();

        builder.Property(surgeryRequest => surgeryRequest.SurgeonId)
            .IsRequired();

        builder.Property(surgeryRequest => surgeryRequest.OperatingRoomId)
            .IsRequired();

        builder.Property(surgeryRequest => surgeryRequest.ProcedureName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(surgeryRequest => surgeryRequest.RequestStatus)
            .IsRequired();
        
        builder.ComplexProperty(
            surgeryRequest => surgeryRequest.RequestedTime,
            requestedTime =>
            {
                requestedTime.Property(timeRange => timeRange.Start)
                    .HasColumnName("RequestedStartTime");

                requestedTime.Property(timeRange => timeRange.End)
                    .HasColumnName("RequestedEndTime");
            });
    }
}