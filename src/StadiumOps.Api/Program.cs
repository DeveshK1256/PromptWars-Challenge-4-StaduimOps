using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Endpoints;
using StadiumOps.Api.Hubs;
using StadiumOps.Api.Middleware;
using StadiumOps.Api.Responses;
using StadiumOps.Application;
using StadiumOps.Infrastructure;
using StadiumOps.Infrastructure.Identity;
using StadiumOps.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "O";
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    // Clear networks and proxies to trust all proxy headers in containerized (Cloud Run) environments
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddFixedWindowLimiter("ai-chat", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        if (builder.Environment.IsProduction())
        {
            origins = ["https://promptwars-challenge-4-staduimops.netlify.app"];
        }
        else if (origins is null || origins.Length == 0)
        {
            origins = ["http://localhost:5173", "http://localhost:3000"];
        }

        policy.WithOrigins(origins)
            .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-Id")
            .WithExposedHeaders("X-Total-Count")
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            title: "Server error",
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            extensions: new Dictionary<string, object?> { ["correlationId"] = context.GetCorrelationId() })
            .ExecuteAsync(context);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("web");
app.UseRateLimiter();

if (app.Environment.IsProduction())
{
    app.UseHsts();
}
app.UseResponseCompression();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) => ApiResults.Ok(context, new
{
    service = "StadiumOps.Api",
    version = "v1",
    status = "running"
})).AllowAnonymous();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

app.MapAuthEndpoints();
app.MapStadiumEndpoints();
app.MapAiEndpoints();
app.MapIncidentEndpoints();
app.MapOperationsEndpoints();
app.MapNotificationEndpoints();
app.MapTransportEndpoints();
app.MapSustainabilityEndpoints();
app.MapVolunteerEndpoints();
app.MapHub<OperationsHub>("/hubs/operations").RequireAuthorization();

app.Run();

public partial class Program;
