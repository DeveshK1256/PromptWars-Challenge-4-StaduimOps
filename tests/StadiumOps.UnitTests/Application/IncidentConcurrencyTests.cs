using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using StadiumOps.Api.Endpoints;
using StadiumOps.Application.Abstractions;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Persistence;
using Xunit;

namespace StadiumOps.UnitTests.Application;

public sealed class IncidentConcurrencyTests
{
    [Fact]
    public async Task UpdateIncident_OnConcurrencyConflict_ReturnsConflictResult()
    {
        // 1. Arrange DB
        var options = new DbContextOptionsBuilder<StadiumOpsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Create a mock dbContext that throws DbUpdateConcurrencyException on SaveChangesAsync
        var mockDbContextOptions = new DbContextOptionsBuilder<StadiumOpsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockDb = new Mock<StadiumOpsDbContext>(mockDbContextOptions);
        mockDb
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Setup mock set for IncidentReports
        var incident = new IncidentReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            Category = "Medical",
            Severity = "High",
            Location = "Section A",
            Description = "Initial details",
            Status = "Open"
        };

        var dbSetMock = new Mock<DbSet<IncidentReport>>();
        // Set up local retrieval
        mockDb.Setup(x => x.IncidentReports).Returns(dbSetMock.Object);
        mockDb
            .Setup(x => x.IncidentReports.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<IncidentReport, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(incident);

        var auditWriterMock = new Mock<IAuditWriter>();
        var outboxWriterMock = new Mock<IIntegrationEventOutboxWriter>();

        var httpContext = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        ]));
        httpContext.User = principal;

        // 2. Act: Invoke the private UpdateStatusAsync method using reflection
        var method = typeof(IncidentEndpoints)
            .GetMethod("UpdateStatusAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(method);

        var resultTask = (Task<IResult>)method.Invoke(null, 
        [
            incident.Id,
            new IncidentStatusRequest("Resolved", "Medical Team"),
            principal,
            mockDb.Object,
            auditWriterMock.Object,
            outboxWriterMock.Object,
            httpContext,
            CancellationToken.None
        ])!;

        var result = await resultTask;

        // 3. Assert
        Assert.NotNull(result);
        // It should return Conflict (which maps to Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)
        var resultTypeName = result.GetType().Name;
        Assert.Contains("Problem", resultTypeName);
    }
}
