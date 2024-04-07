using ChatService.Extensions;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;

[Authorize]
internal sealed class ChatHub : Hub {
    private readonly ChatsService _service;
    private readonly HttpContext _context;

    public ChatHub(ChatsService service) {
        _service = service;
        _context = Context.GetHttpContext()!;
    }

    public async Task JoinChat(string chatId) {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SendMessage(string chatId, string content) { 
        _ = _context.TryGetUserId(out var userId);
        var message = new Message(chatId, userId, content);
        var result = await _service.SendMessageAsync(message);

        if (!result.IsSuccess) {
            throw new HubException(result.Exceptions[0].ToString());
        }

        await Clients.Group(chatId).SendAsync("", message);
    }

    public async Task UpdateMessage(string chatId, string messageId, string content) {
        var result = await _service.UpdateMessageAsync(chatId, messageId, content);

        if (!result.IsSuccess) {
            throw new HubException(result.Exceptions[0].ToString());
        }

        await Clients.Group(chatId).SendAsync("", messageId, content);
    }

    public async Task DeleteMessage(string chatId, string messageId) {
        var result = await _service.DeleteMessageAsync(chatId, messageId);

        if (!result.IsSuccess) {
            throw new HubException(result.Exceptions[0].ToString());
        }

        await Clients.Group(chatId).SendAsync("", messageId);
    }
}