using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Caching.Memory;
using StadiumOps.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Security;
using StadiumOps.Infrastructure.Identity;
using StadiumOps.Infrastructure.Integrations;
using StadiumOps.Infrastructure.Persistence;
using StadiumOps.Infrastructure.Security;

namespace StadiumOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMemoryCache();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(x => x.AccessTokenMinutes > 0, "Jwt:AccessTokenMinutes must be greater than zero.")
            .Validate(x => x.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be greater than zero.")
            .ValidateOnStart();
        services.Configure<VertexAiOptions>(configuration.GetSection("VertexAI"));
        services.Configure<FirebaseOptions>(configuration.GetSection("Firebase"));
        services.AddSingleton(provider =>
        {
            var firebaseOpts = provider.GetRequiredService<IOptions<FirebaseOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(firebaseOpts.ServiceAccountPath) && File.Exists(firebaseOpts.ServiceAccountPath))
            {
                return FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(firebaseOpts.ServiceAccountPath),
                    ProjectId = firebaseOpts.ProjectId
                });
            }
            return FirebaseApp.DefaultInstance!;
        });
        services.Configure<MapsOptions>(configuration.GetSection("GoogleMaps"));

        var provider = configuration["Persistence:Provider"];
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = environment.IsDevelopment() ? "InMemory" : "SqlServer";
        }

        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<StadiumOpsDbContext>(options =>
                options.UseInMemoryDatabase("StadiumOps"));
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("StadiumOps");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:StadiumOps is required when Persistence:Provider is SqlServer.");
            }

            services.AddDbContext<StadiumOpsDbContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported Persistence:Provider '{provider}'. Use InMemory or SqlServer.");
        }

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<StadiumOpsDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = environment.IsProduction();
            });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, configuredJwt) =>
            {
                var jwt = configuredJwt.Value;
                var signingKey = jwt.ResolveSigningKey(environment.IsProduction());
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("OperationsAccess", policy => policy.RequireRole(StadiumRoles.OperationsAccess))
            .AddPolicy("IncidentAccess", policy => policy.RequireRole(StadiumRoles.IncidentAccess))
            .AddPolicy("AdminOnly", policy => policy.RequireRole(StadiumRoles.Admin, StadiumRoles.SuperAdmin));

        services.AddScoped<TokenService>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IIntegrationEventOutboxWriter, IntegrationEventOutboxWriter>();
        services.AddScoped<IAiAssistantGateway, VertexGeminiAssistantGateway>();
        services.AddScoped<INotificationGateway, FirebaseNotificationGateway>();
        
        services.AddHealthChecks()
            .AddCheck<DatabaseReadinessCheck>("database");
            
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }
}
