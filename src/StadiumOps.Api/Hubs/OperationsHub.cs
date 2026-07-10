using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StadiumOps.Api.Hubs;

[Authorize]
public sealed class OperationsHub : Hub
{
    public Task SubscribeToStadium(string stadiumId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"stadium:{stadiumId}");
    }

    public Task SubscribeToOperations()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "operations");
    }
}
