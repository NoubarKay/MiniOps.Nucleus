using Microsoft.AspNetCore.SignalR;

namespace Nucleus.Core.Hubs;

public class NucleusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveMessage", "Connected to MiniOps MetricsHub");
        await base.OnConnectedAsync();
    }
}