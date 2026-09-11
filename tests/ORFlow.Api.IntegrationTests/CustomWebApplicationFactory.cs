using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ORFlow.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace ORFlow.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServerContainer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ORFlowDbContext>>();

            services.AddDbContext<ORFlowDbContext>(options =>
                options.UseSqlServer(
                    _sqlServerContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _sqlServerContainer.StartAsync();

        using IServiceScope scope =
            Services.CreateScope();

        ORFlowDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<ORFlowDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlServerContainer.DisposeAsync();
    }
}
