using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.BackgroundServices;
using StadiumOps.Infrastructure.Persistence;
using Xunit;

namespace StadiumOps.UnitTests.BackgroundServices;

public sealed class BackgroundServiceTests
{
    [Fact]
    public async Task OutboxPublisherService_ProcessesPendingEvents()
    {
        // 1. Arrange DB
        var options = new DbContextOptionsBuilder<StadiumOpsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new StadiumOpsDbContext(options);
        var pendingEvent = new IntegrationEventOutbox
        {
            EventType = "TestEvent",
            AggregateType = "Match",
            AggregateId = Guid.NewGuid(),
            PayloadJson = "{}"
        };
        dbContext.IntegrationEventOutbox.Add(pendingEvent);
        await dbContext.SaveChangesAsync();

        // 2. Setup mock dependencies
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(StadiumOpsDbContext)))
            .Returns(dbContext);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(serviceScopeMock.Object);

        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<OutboxPublisherService>>();

        var service = new OutboxPublisherService(
            scopeFactoryMock.Object,
            configurationMock.Object,
            loggerMock.Object);

        // 3. Act: We run the processing method using reflection since it is a protected loop,
        // or we call it by triggering StartAsync/StopAsync briefly.
        // Let's invoke the process method directly or trigger a one-shot delay loop.
        var method = typeof(OutboxPublisherService)
            .GetMethod("ProcessPendingEventsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(method);
        var task = (Task)method.Invoke(service, [CancellationToken.None])!;
        await task;

        // 4. Assert: Status changed to Published
        var updatedEvent = await dbContext.IntegrationEventOutbox.FirstAsync();
        Assert.Equal("Published", updatedEvent.PublishStatus);
        Assert.NotNull(updatedEvent.PublishedAt);
    }
}
