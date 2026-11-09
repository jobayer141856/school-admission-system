using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SchoolAdmission.API.Middleware;
using SchoolAdmission.Application;
using SchoolAdmission.Infrastructure;
using SchoolAdmission.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add Application & Infrastructure Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add ProblemDetails & Centralized Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

// Configure Controllers
builder.Services.AddControllers();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<SchoolAdmission.API.Health.DatabaseHealthCheck>("database_health_check", tags: new[] { "ready", "db" });

// Configure OpenAPI & Swagger
builder.Services.AddOpenApi();

// Configure CORS
const string corsPolicyName = "AllowFrontend";
var allowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Map /swagger to Swagger UI pointing to /openapi/v1.json
    app.MapGet("/swagger", () => Results.Content("""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <title>School Admission System API - Swagger</title>
            <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
        </head>
        <body>
            <div id="swagger-ui"></div>
            <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
            <script>
                window.onload = () => {
                    window.ui = SwaggerUIBundle({
                        url: '/openapi/v1.json',
                        dom_id: '#swagger-ui',
                    });
                };
            </script>
        </body>
        </html>
        """, "text/html"));
}

app.UseCors(corsPolicyName);

// Health check endpoint
app.MapHealthChecks("/health");

// Map API Controllers
app.MapControllers();

// Apply DB migrations with resilience on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolAdmissionDbContext>();

    try
    {
        logger.LogInformation("Checking database connection and applying any pending migrations...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database is connected and migrations are up to date.");
        }
        else
        {
            logger.LogWarning("Database is not reachable on startup; health check will report unhealthy until connected.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not initialize database on startup. Health check will monitor connection status.");
    }
}

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
