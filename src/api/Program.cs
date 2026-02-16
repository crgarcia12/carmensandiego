using Api.Endpoints;
using Api.Middleware;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Register game services
builder.Services.AddSingleton<GameDataProvider>();
builder.Services.AddSingleton<IGameSessionService, InMemoryGameSessionService>();
builder.Services.AddSingleton<ICaseService, CaseService>();
builder.Services.AddSingleton<INpcChatService, NpcChatService>();
builder.Services.AddSingleton<ISuspectService, SuspectService>();

// Keep legacy services for backward compatibility
builder.Services.AddSingleton<Api.Services.ISessionService, Api.Services.Mock.MockSessionService>();
builder.Services.AddSingleton<IConversationHandler, PlaceholderConversationHandler>();

var app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Session middleware for /api/cases/* routes
app.UseMiddleware<SessionMiddleware>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("Health");

// API version
app.MapGet("/api/info", () => Results.Ok(new { version = "1.0.0", framework = "spec2cloud" }))
   .WithName("ApiInfo")
   .WithTags("Info");

// Game endpoints
app.MapGameSessionEndpoints();
app.MapCaseEndpoints();
app.MapCityEndpoints();
app.MapNpcEndpoints();
app.MapSuspectEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
