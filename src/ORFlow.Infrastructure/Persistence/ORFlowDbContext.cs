using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ORFlow.Domain.SurgeryRequests;
using ORFlow.Infrastructure.Identity;
using ORFlow.Infrastructure.Persistence.Configurations;

namespace ORFlow.Infrastructure.Persistence;

public sealed class ORFlowDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ORFlowDbContext(DbContextOptions<ORFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<SurgeryRequest> SurgeryRequests => Set<SurgeryRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SurgeryRequestConfiguration());
    }
}
