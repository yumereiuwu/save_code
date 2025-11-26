using CodeShowcase.Api;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

builder.Services.AddSingleton<ICodeRepository>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var configuredPath = configuration["CodeDataPath"]
                         ?? Environment.GetEnvironmentVariable("CODE_DATA_PATH");

    var rootBasedPath = configuredPath ?? Path.Combine(env.ContentRootPath, "../../data/codes.json");
    return new FileCodeRepository(rootBasedPath);
});

var app = builder.Build();
app.UseCors();

var api = app.MapGroup("/api/codes");

app.MapGet("/", () =>
    Results.Ok(new
    {
        message = "Code Showcase backend running",
        endpoints = new[] { "/api/codes", "/hub/codes" }
    }));

api.MapGet("/", async (ICodeRepository repo, CancellationToken token) =>
    Results.Ok(await repo.GetAllAsync(token)));

api.MapGet("/{id}", async (string id, ICodeRepository repo, CancellationToken token) =>
{
    var entry = await repo.GetAsync(id, token);
    return entry is not null ? Results.Ok(entry) : Results.NotFound();
});

api.MapPost("/", async (CreateCodeRequest request, ICodeRepository repo, IHubContext<CodeHub> hub, CancellationToken token) =>
{
    var entry = new CodeEntry(
        request.Id ?? Guid.NewGuid().ToString("N"),
        request.Title,
        request.Language,
        request.Content,
        DateTimeOffset.UtcNow);

    await repo.AddAsync(entry, token);
    await hub.Clients.All.SendAsync(CodeHub.EventName, cancellationToken: token);
    return Results.Created($"/api/codes/{entry.Id}", entry);
});

api.MapDelete("/{id}", async (string id, ICodeRepository repo, IHubContext<CodeHub> hub, CancellationToken token) =>
{
    var removed = await repo.DeleteAsync(id, token);
    if (!removed)
    {
        return Results.NotFound();
    }

    await hub.Clients.All.SendAsync(CodeHub.EventName, cancellationToken: token);
    return Results.NoContent();
});

app.MapHub<CodeHub>("/hub/codes");

app.Run();

internal record CreateCodeRequest(
    [property: JsonRequired] string Title,
    [property: JsonRequired] string Language,
    [property: JsonRequired] string Content,
    string? Id);

