var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Get config from environment (ConfigMap)
var appName = Environment.GetEnvironmentVariable("APP_NAME") ?? "ACR Pipeline Demo";
var appVersion = Environment.GetEnvironmentVariable("APP_VERSION") ?? "1.0.0";
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information";

// Endpoints
app.MapGet("/", () => Results.Ok(new
{
    message = $"Welcome to {appName}",
    environment = environment,
    timestamp = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/api/info", () => Results.Ok(new
{
    application = appName,
    version = appVersion,
    environment = environment,
    logLevel = logLevel,
    machineName = Environment.MachineName
}));

app.MapGet("/api/config", () => Results.Ok(new
{
    APP_NAME = appName,
    APP_VERSION = appVersion,
    ENVIRONMENT = environment,
    LOG_LEVEL = logLevel
}));

app.Run();
