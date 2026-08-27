using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Domain.SurgeryRequests;

namespace ORFlow.Infrastructure.Persistence.Repositories;

public sealed class SurgeryRequestRepository : ISurgeryRequestRepository
{
    private readonly ORFlowDbContext _dbContext;

    public SurgeryRequestRepository(ORFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddAsync(SurgeryRequest surgeryRequest)
{
    await _dbContext.SurgeryRequests.AddAsync(surgeryRequest);
    await _dbContext.SaveChangesAsync();
}
    public async Task<SurgeryRequest?> GetSurgeryRequestByIdAsync(
        Guid surgeryRequestId)
    {
        return await _dbContext.SurgeryRequests.FindAsync(surgeryRequestId);
    }

}