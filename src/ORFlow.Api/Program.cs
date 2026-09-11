using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ORFlow.Api.ErrorHandling;
using ORFlow.Api.Contracts.SurgeryRequests;
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

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
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

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// Authentication endpoints.
app.MapGroup("/auth")
    .MapIdentityApi<ApplicationUser>();

// Public health endpoint.
app.MapGet("/health", () => "ORFlow API is running.");

// Surgery request endpoints.
app.MapPost("/surgery-requests", async (
    CreateSurgeryRequestRequest request,
    CreateSurgeryRequestHandler handler
) =>
{
    var command = new CreateSurgeryRequestCommand(
        request.PatientId,
        request.SurgeonId,
        request.OperatingRoomId,
        request.ProcedureName,
        request.RequestedStartTime,
        request.RequestedEndTime);

    SurgeryRequest surgeryRequest = await handler.HandleAsync(command);

    return Results.Created(
        $"/surgery-requests/{surgeryRequest.SurgeryRequestId}",
        ToResponse(surgeryRequest));
})
.RequireAuthorization("CanCreateSurgeryRequest");

app.MapGet("/surgery-requests/{id:guid}", async (
    Guid id,
    GetSurgeryRequestByIdHandler handler
) =>
{
    SurgeryRequest? surgeryRequest = await handler.HandleAsync(id);

    if (surgeryRequest is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(ToResponse(surgeryRequest));
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

    return Results.Ok(ToResponse(surgeryRequest));
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
            surgeryRequest = ToResponse(result.SurgeryRequest)
        });
    }

    return Results.Ok(ToResponse(result.SurgeryRequest));
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

    return Results.Ok(ToResponse(surgeryRequest));
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

    return Results.Ok(ToResponse(surgeryRequest));
})
.RequireAuthorization("CanManageSurgeryRequest");

app.Run();

static SurgeryRequestResponse ToResponse(SurgeryRequest surgeryRequest)
{
    return new SurgeryRequestResponse(
        surgeryRequest.SurgeryRequestId,
        surgeryRequest.PatientId,
        surgeryRequest.SurgeonId,
        surgeryRequest.OperatingRoomId,
        surgeryRequest.ProcedureName,
        surgeryRequest.RequestedTime.Start,
        surgeryRequest.RequestedTime.End,
        surgeryRequest.RequestStatus);
}

public partial class Program { }
