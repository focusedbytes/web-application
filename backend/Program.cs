using FluentValidation;
using FluentValidation.AspNetCore;
using FocusedBytes.Api.Application.Users.CommandHandlers;
using FocusedBytes.Api.Application.Users.EventHandlers;
using FocusedBytes.Api.Application.Users.QueryHandlers;
using FocusedBytes.Api.Infrastructure.EventStore;
using FocusedBytes.Api.Infrastructure.ReadModels;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting FocusedBytes API application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Global Exception Handler
builder.Services.AddExceptionHandler<FocusedBytes.Api.Infrastructure.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register FluentValidation
// Scans the assembly for all AbstractValidator<T> classes and registers them with the DI container
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add FluentValidation Auto Validation
// Configures ASP.NET Core to automatically validate request DTOs using FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Database contexts
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EventStoreDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ReadModelDbContext>(options =>
    options.UseNpgsql(connectionString));

// Event Store
builder.Services.AddScoped<EventBus>();
builder.Services.AddScoped<EventStoreRepository>();

// Command Handlers
builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();
builder.Services.AddScoped<UpdateProfileCommandHandler>();
builder.Services.AddScoped<AddAuthMethodCommandHandler>();
builder.Services.AddScoped<UpdateAuthMethodCommandHandler>();
builder.Services.AddScoped<RemoveAuthMethodCommandHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();
builder.Services.AddScoped<DeactivateUserCommandHandler>();

// Query Handlers
builder.Services.AddScoped<GetUsersQueryHandler>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();

// Event Handlers
builder.Services.AddScoped<UserEventHandler>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString ?? throw new InvalidOperationException("Database connection string is not configured"),
        name: "PostgreSQL Database",
        tags: new[] { "database", "postgresql" });

var app = builder.Build();

// Run database migrations on startup (skip during design-time/EF tools execution)
var isDesignTime = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "DesignTime" ||
                   args.Contains("--no-build") ||
                   AppDomain.CurrentDomain.GetData("DesignTime") != null;

if (!isDesignTime)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            Log.Information("Running database migrations...");

            var eventStoreContext = services.GetRequiredService<EventStoreDbContext>();
            await eventStoreContext.Database.MigrateAsync();
            Log.Information("EventStore migrations completed successfully");

            var readModelContext = services.GetRequiredService<ReadModelDbContext>();
            await readModelContext.Database.MigrateAsync();
            Log.Information("ReadModel migrations completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}

// Configure the HTTP request pipeline

// Add exception handling middleware (must be early in the pipeline)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Serilog request logging
app.UseSerilogRequestLogging();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // Just check if the app is running
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

Log.Information("FocusedBytes API started successfully");

app.Run();
}
catch (Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("Shutting down FocusedBytes API");
    Log.CloseAndFlush();
}
