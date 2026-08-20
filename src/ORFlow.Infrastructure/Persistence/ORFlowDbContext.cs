using Microsoft.EntityFrameworkCore;
using ORFlow.Domain.SurgeryRequests;
using ORFlow.Infrastructure.Persistence.Configurations;

namespace ORFlow.Infrastructure.Persistence;

public sealed class ORFlowDbContext : DbContext
{
    public ORFlowDbContext(DbContextOptions<ORFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<SurgeryRequest> SurgeryRequests => Set<SurgeryRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SurgeryRequestConfiguration());
    }
}