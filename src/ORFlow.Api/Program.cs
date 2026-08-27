using Microsoft.EntityFrameworkCore;
using ORFlow.Application.SurgeryRequests.Approve;
using ORFlow.Application.SurgeryRequests.Common;
using ORFlow.Application.SurgeryRequests.Create;
using ORFlow.Application.SurgeryRequests.GetById;
using ORFlow.Domain.SurgeryRequests;
using ORFlow.Infrastructure.Persistence;
using ORFlow.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ORFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ORFlowDatabase")));
builder.Services.AddScoped<ISurgeryRequestRepository, SurgeryRequestRepository>();
builder.Services.AddScoped<CreateSurgeryRequestHandler>();
builder.Services.AddScoped<GetSurgeryRequestByIdHandler>();
builder.Services.AddScoped<ApproveSurgeryRequestHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => "ORFlow API is running.");

app.MapPost("/surgery-requests", async (
    CreateSurgeryRequestCommand command,
    CreateSurgeryRequestHandler handler
) =>
{
    var surgeryRequest = await handler.HandleAsync(command);

    return Results.Created(
        $"/surgery-requests/{surgeryRequest.SurgeryRequestId}",
        surgeryRequest);
});

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
});

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
});

app.Run();
