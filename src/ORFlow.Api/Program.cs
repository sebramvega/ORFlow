using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ORFlow.Application.SurgeryRequests.Approve;
using ORFlow.Application.SurgeryRequests.Archive;
using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Application.SurgeryRequests.Complete;
using ORFlow.Application.SurgeryRequests.Create;
using ORFlow.Application.SurgeryRequests.GetById;
using ORFlow.Application.SurgeryRequests.Schedule;
using ORFlow.Domain.SurgeryRequests;
using ORFlow.Infrastructure.Identity;
using ORFlow.Infrastructure.Persistence;
using ORFlow.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ORFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ORFlowDatabase")));

builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ORFlowDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanCreateSurgeryRequest", policy =>
        policy.RequireRole(
            ApplicationRoles.Surgeon,
            ApplicationRoles.Administrator));

    options.AddPolicy("CanManageSurgeryRequest", policy =>
        policy.RequireRole(
            ApplicationRoles.Scheduler,
            ApplicationRoles.Administrator));
});

builder.Services.AddScoped<ISurgeryRequestRepository, SurgeryRequestRepository>();
builder.Services.AddScoped<CreateSurgeryRequestHandler>();
builder.Services.AddScoped<GetSurgeryRequestByIdHandler>();
builder.Services.AddScoped<ApproveSurgeryRequestHandler>();
builder.Services.AddScoped<ScheduleSurgeryRequestHandler>();
builder.Services.AddScoped<CompleteSurgeryRequestHandler>();
builder.Services.AddScoped<ArchiveSurgeryRequestHandler>();

var app = builder.Build();

// Initialize the database and Identity roles.
using (IServiceScope scope = app.Services.CreateScope())
{
    ORFlowDbContext dbContext =
        scope.ServiceProvider.GetRequiredService<ORFlowDbContext>();

    await dbContext.Database.EnsureCreatedAsync();

    RoleManager<IdentityRole<Guid>> roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await IdentityInitializer.InitializeRolesAsync(roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Authentication endpoints.
app.MapGroup("/auth")
    .MapIdentityApi<ApplicationUser>();

// Public health endpoint.
app.MapGet("/health", () => "ORFlow API is running.");

// Surgery request endpoints.
app.MapPost("/surgery-requests", async (
    CreateSurgeryRequestCommand command,
    CreateSurgeryRequestHandler handler
) =>
{
    var surgeryRequest = await handler.HandleAsync(command);

    return Results.Created(
        $"/surgery-requests/{surgeryRequest.SurgeryRequestId}",
        surgeryRequest);
})
.RequireAuthorization("CanCreateSurgeryRequest");

app.MapGet("/surgery-requests/{id:guid}", async (
    Guid id,
    GetSurgeryRequestByIdHandler handler
) =>
{
    var surgeryRequest = await handler.HandleAsync(id);

    if (surgeryRequest is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(surgeryRequest);
})
.RequireAuthorization();

app.MapPost("/surgery-requests/{id:guid}/approve", async (
    Guid id,
    ApproveSurgeryRequestHandler handler
) =>
{
    SurgeryRequest? surgeryRequest = await handler.HandleAsync(id);

    if (surgeryRequest is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(surgeryRequest);
})
.RequireAuthorization("CanManageSurgeryRequest");

app.MapPost("/surgery-requests/{id:guid}/schedule", async (
    Guid id,
    ScheduleSurgeryRequestHandler handler
) =>
{
    ScheduleSurgeryRequestResult result = await handler.HandleAsync(id);

    if (result.SurgeryRequest is null)
    {
        return Results.NotFound();
    }

    if (result.HasConflict)
    {
        return Results.Conflict(new
        {
            message = "Scheduling conflict detected.",
            surgeryRequest = result.SurgeryRequest
        });
    }

    return Results.Ok(result.SurgeryRequest);
})
.RequireAuthorization("CanManageSurgeryRequest");

app.MapPost("/surgery-requests/{id:guid}/complete", async (
    Guid id,
    CompleteSurgeryRequestHandler handler
) =>
{
    SurgeryRequest? surgeryRequest = await handler.HandleAsync(id);

    if (surgeryRequest is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(surgeryRequest);
})
.RequireAuthorization("CanManageSurgeryRequest");

app.MapPost("/surgery-requests/{id:guid}/archive", async (
    Guid id,
    ArchiveSurgeryRequestHandler handler
) =>
{
    SurgeryRequest? surgeryRequest = await handler.HandleAsync(id);

    if (surgeryRequest is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(surgeryRequest);
})
.RequireAuthorization("CanManageSurgeryRequest");

app.Run();

public partial class Program { }
