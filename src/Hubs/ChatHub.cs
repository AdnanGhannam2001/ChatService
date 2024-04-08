using ChatService.Extensions;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PR2.Shared.Enums;

namespace ChatService.Hubs;

[Authorize]
internal sealed class ChatHub : Hub {
    private const string MessageSent    = nameof(MessageSent);
    private const string MessageUpdated = nameof(MessageUpdated);
    private const string MessageDeleted = nameof(MessageDeleted);

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

        var memberResult = await _service.GetMemberAsync(chatId, userId);

        if (!memberResult.IsSuccess) {
            throw new HubException(memberResult.Exceptions[0].ToString());
        }
        
        var message = new Message(chatId, userId, content);
        var result = await _service.SendMessageAsync(message);

        if (!result.IsSuccess) {
            throw new HubException(result.Exceptions[0].ToString());
        }

        await Clients.Group(chatId).SendAsync(MessageSent, message);
    }

    public async Task UpdateMessage(string chatId, string messageId, string content) {
        _ = _context.TryGetUserId(out var userId);

        await AuthorizeUserAsync(chatId, userId, messageId);

        var result = await _service.UpdateMessageAsync(chatId, messageId, content);

        if (!result.IsSuccess) {
            throw new HubException(result.Exceptions[0].ToString());
        }

        await Clients.Group(chatId).SendAsync(MessageUpdated, messageId, content);
    }

    public async Task DeleteMessage(string chatId, string messageId) {
        _ = _context.TryGetUserId(out var userId);

        await AuthorizeUserAsync(chatId, userId, messageId);

        var result = await _service.DeleteMessageAsync(chatId, messageId);

        if (!result.IsSuccess) {
            throw new HubException(result.Exceptions[0].ToString());
        }

        await Clients.Group(chatId).SendAsync(MessageDeleted, messageId);
    }

    private async Task AuthorizeUserAsync(string chatId, string userId, string messageId) {
        var memberResult = await _service.GetMemberAsync(chatId, userId);

        if (!memberResult.IsSuccess) {
            throw new HubException(memberResult.Exceptions[0].ToString());
        }

        var messageResult = await _service.GetMessageByIdAsync(messageId);

        if (!messageResult.IsSuccess) {
            throw new HubException(messageResult.Exceptions[0].ToString());
        }

        if (messageResult.Value.SenderId != userId
            && !ChatsService.HasMinimalRole(memberResult.Value.Role, MemberRoleTypes.Organizer))
        {
            throw new HubException("You're not allowed preform this action");
        }
    }
}