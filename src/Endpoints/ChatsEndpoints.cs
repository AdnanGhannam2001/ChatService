using ChatService.Data.Models;
using ChatService.Dtos;
using ChatService.Extensions;
using ChatService.Hubs;
using ChatService.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PR2.Shared.Common;
using static ChatService.Constants.Policies;

namespace ChatService.Endpoints;

internal static class ChatsEndpoints
{
    public static RouteGroupBuilder MapChatEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("", GetChatsPage);
        group.MapGet("{id}", GetMessagesPage).RequireAuthorization(UserInChat);
        group.MapPost("{id}/join", JoinChat).RequireAuthorization(UserInChat);
        group.MapPost("{id}", SendMessage).RequireAuthorization(UserInChat);
        group.MapPatch("{id}/{messageId}", UpdateMessage).RequireAuthorization(OrganizerInChat);
        group.MapDelete("{id}/{messageId}", DeleteMessage).RequireAuthorization(OrganizerInChat);

        return group;
    }

    private static async Task<Ok<Page<Chat>>> GetChatsPage(HttpContext context,
        [FromServices] IChatsService service,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool desc = true)
    {
        _ = context.TryGetUserId(out var userId);

        var page = await service.GetChatsPageAsync(userId, pageNumber, pageSize, desc);

        return TypedResults.Ok(page);
    }

    private static async Task<Results<Ok<Page<Message>>, BadRequest<ExceptionBase[]>>> GetMessagesPage(HttpContext context,
        [FromServices] IChatsService service,
        [FromRoute] string id,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool desc = true)
    {
        _ = context.TryGetUserId(out var userId);

        var result = await service.GetMessagesPageAsync(id, pageNumber, pageSize, desc);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result.Exceptions);
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok, BadRequest<ExceptionBase[]>>> JoinChat([FromServices] IHubContext<ChatHub, IChatClient> hub,
        [FromRoute(Name = "id")] string chatId,
        [FromQuery] string connectionId)
    {
        await hub.Groups.AddToGroupAsync(connectionId, chatId);
        return TypedResults.Ok();
    }


    private static async Task<Results<Ok<Message>, BadRequest<ExceptionBase[]>>> SendMessage(HttpContext context,
        [FromServices] IChatsService service,
        [FromServices] IHubContext<ChatHub, IChatClient> hub,
        [FromRoute(Name = "id")] string chatId,
        [FromBody] SendMessageRequest dto)
    {
        context.TryGetUserId(out var userId);

        var message = new Message(chatId, userId, dto.Content);
        var result = await service.SendMessageAsync(message);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result.Exceptions);
        }

        await hub.Clients.Group(chatId).MessageSent(message);
        return TypedResults.Ok(message);
    }

    private static async Task<Results<Ok, BadRequest<ExceptionBase[]>>> UpdateMessage([FromServices] IChatsService service,
        [FromServices] IHubContext<ChatHub, IChatClient> hub,
        [FromRoute(Name = "id")] string chatId,
        [FromRoute] string messageId,
        [FromBody] SendMessageRequest dto)
    {
        var result = await service.UpdateMessageAsync(chatId, messageId, dto.Content);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result.Exceptions);
        }

        await hub.Clients.Group(chatId).MessageUpdated(messageId, dto.Content);

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<ExceptionBase[]>>> DeleteMessage([FromServices] IChatsService service,
        [FromServices] IHubContext<ChatHub, IChatClient> hub,
        [FromRoute(Name = "id")] string chatId,
        [FromRoute] string messageId)
    {
        var result = await service.DeleteMessageAsync(chatId, messageId);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result.Exceptions);
        }

        await hub.Clients.Group(chatId).MessageDeleted(messageId);

        return TypedResults.Ok();
    }
}