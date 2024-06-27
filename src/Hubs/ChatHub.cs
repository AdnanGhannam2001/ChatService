using ChatService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;

[Authorize]
internal sealed class ChatHub : Hub<IChatClient>
{
    public Task<string> GetConnectionId() => Task.FromResult(Context.ConnectionId);
}