using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Application.SurgeryRequests.Common;

public interface ISurgeryRequestRepository
{
    Task AddAsync(SurgeryRequest surgeryRequest);
}