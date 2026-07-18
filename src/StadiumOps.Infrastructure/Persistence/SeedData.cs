using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StadiumOps.Application.Security;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Identity;

namespace StadiumOps.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(
        StadiumOpsDbContext dbContext,
        RoleManager<ApplicationRole> roleManager,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsInMemory())
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        await SeedRolesAsync(roleManager);

        if (await dbContext.Stadiums.AnyAsync(cancellationToken))
        {
            await SeedAiKnowledgeDocumentsAsync(dbContext, cancellationToken);
            return;
        }

        var stadium = new Stadium
        {
            Id = Guid.Parse("3da80b77-2580-4dc9-9df1-3f478f86ef49"),
            Name = "MetLife Stadium",
            City = "East Rutherford",
            Country = "United States",
            Capacity = 82500,
            Latitude = 40.8135m,
            Longitude = -74.0745m,
            TimeZone = "America/New_York"
        };

        dbContext.Stadiums.Add(stadium);
        dbContext.Matches.Add(new Match
        {
            StadiumId = stadium.Id,
            HomeTeam = "USA",
            AwayTeam = "Canada",
            Stage = "Group Stage",
            StartsAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(23),
            Status = "Scheduled"
        });

        dbContext.PointsOfInterest.AddRange(
            new StadiumPointOfInterest
            {
                StadiumId = stadium.Id,
                Name = "Accessible Gate C",
                Category = "Entrance",
                Level = "Ground",
                Zone = "North",
                IsAccessible = true,
                EstimatedWaitMinutes = 6
            },
            new StadiumPointOfInterest
            {
                StadiumId = stadium.Id,
                Name = "Global Food Hall",
                Category = "Food",
                Level = "2",
                Zone = "East",
                IsAccessible = true,
                EstimatedWaitMinutes = 12
            },
            new StadiumPointOfInterest
            {
                StadiumId = stadium.Id,
                Name = "Medical Station 204",
                Category = "Medical",
                Level = "2",
                Zone = "West",
                IsAccessible = true,
                EstimatedWaitMinutes = 0
            });

        dbContext.NavigationRoutes.AddRange(
            new NavigationRoute
            {
                StadiumId = stadium.Id,
                FromLocation = "Metro Shuttle Drop",
                ToLocation = "Accessible Gate C",
                DistanceMeters = 420,
                EstimatedMinutes = 7,
                IsAccessible = true,
                CrowdLoadPercent = 38,
                SafetyNote = "Accessible route with curb-free access and staff checkpoints."
            },
            new NavigationRoute
            {
                StadiumId = stadium.Id,
                FromLocation = "Gate A",
                ToLocation = "Section 214",
                DistanceMeters = 280,
                EstimatedMinutes = 5,
                IsAccessible = false,
                CrowdLoadPercent = 61,
                SafetyNote = "Use escalator bank A2; avoid this route during emergency egress."
            });

        dbContext.CrowdZones.AddRange(
            new CrowdZone
            {
                StadiumId = stadium.Id,
                Name = "North Concourse",
                CurrentDensity = 62,
                MaximumCapacity = 100,
                Status = "Elevated"
            },
            new CrowdZone
            {
                StadiumId = stadium.Id,
                Name = "East Food Hall",
                CurrentDensity = 78,
                MaximumCapacity = 100,
                Status = "Congested"
            });

        dbContext.TransportStatuses.AddRange(
            new TransportStatus
            {
                Mode = "Shuttle",
                Provider = "Meadowlands Event Shuttle",
                Status = "On schedule",
                EstimatedDelayMinutes = 0
            },
            new TransportStatus
            {
                Mode = "Parking",
                Provider = "Lot E",
                Status = "Limited spaces",
                EstimatedDelayMinutes = 8
            });

        dbContext.SustainabilityMetrics.Add(new SustainabilityMetric
        {
            StadiumId = stadium.Id,
            EnergyKwh = 18400,
            WaterLiters = 52000,
            WasteKg = 8900,
            RecyclingRate = 68,
            CarbonScore = 82,
            MetricDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        dbContext.VolunteerTasks.AddRange(
            new VolunteerTask
            {
                VolunteerUserId = Guid.Parse("b114d101-de3f-42e5-bc09-9069695662bb"),
                Title = "Assist Fans with Navigation at Section 102",
                Location = "Section 102 Entrance",
                Status = "Assigned",
                Priority = "Normal",
                StartsAt = DateTimeOffset.UtcNow,
                EndsAt = DateTimeOffset.UtcNow.AddHours(4)
            },
            new VolunteerTask
            {
                VolunteerUserId = Guid.Parse("b114d101-de3f-42e5-bc09-9069695662bb"),
                Title = "Distribute Recycling Bags at Gate C",
                Location = "Gate C",
                Status = "InProgress",
                Priority = "High",
                StartsAt = DateTimeOffset.UtcNow.AddHours(-1),
                EndsAt = DateTimeOffset.UtcNow.AddHours(3)
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        await SeedAiKnowledgeDocumentsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in StadiumRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = role,
                NormalizedName = role.ToUpperInvariant(),
                Description = $"{role} access role for the smart stadium platform."
            });

            if (!result.Succeeded)
            {
                var message = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to seed role {role}: {message}");
            }
        }
    }

    private static async Task SeedAiKnowledgeDocumentsAsync(
        StadiumOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (await dbContext.AiKnowledgeDocuments.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.AiKnowledgeDocuments.AddRange(
            new AiKnowledgeDocument
            {
                Category = "Accessibility",
                Title = "Demo Accessibility Routing Guide",
                SourceType = "DemoSeed",
                ContentSummary = "Use accessible gates, elevators, and staff checkpoints for wheelchair and mobility-support routes. Never recommend routes marked inaccessible.",
                Language = "en",
                IsApproved = true
            },
            new AiKnowledgeDocument
            {
                Category = "Emergency Procedures",
                Title = "Demo Emergency Escalation Guidance",
                SourceType = "DemoSeed",
                ContentSummary = "AI guidance is advisory. Medical, fire, security, and evacuation issues must be escalated to venue responders and official protocols.",
                Language = "en",
                IsApproved = true
            },
            new AiKnowledgeDocument
            {
                Category = "Transportation",
                Title = "Demo Transportation Assistance Guide",
                SourceType = "DemoSeed",
                ContentSummary = "Recommend shuttles, public transit, and accessible pickup areas when configured data is available. Clearly state when live transport data is missing.",
                Language = "en",
                IsApproved = true
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
