using ChatService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;

[Authorize]
internal sealed class ChatHub : Hub<IChatClient>
{
    public async Task GetConnectionId() => await Clients.Caller.SendConnectionId(Context.ConnectionId);
}