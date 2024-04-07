using ChatService.Extensions;
using ChatService.Interfaces;
using ChatService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PR2.Shared.Common;

namespace ChatService.Endpoints;

internal static class ChatsEndpoints {
    public static RouteGroupBuilder MapChatEndpoints(this RouteGroupBuilder group) {
        group.MapGet("", GetChatsPage);
        group.MapGet("{id}", GetMessagesPage);

        return group;
    }

    private static async Task<Ok<Page<Chat>>> GetChatsPage(HttpContext context,
        [FromServices] IChatsService service,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10)
    {
        _ = context.TryGetUserId(out var userId); // Should be always true in this case

        var page = await service.GetChatsPageAsync(userId, pageNumber, pageSize, true);

        return TypedResults.Ok(page);
    }

    // TODO: Add Membership Check
    private static async Task<Results<Ok<Page<Message>>, BadRequest<ExceptionBase[]>>> GetMessagesPage(HttpContext context,
        [FromServices] IChatsService service,
        [FromRoute] string id,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10)
    {
        _ = context.TryGetUserId(out var userId);

        var pageResult = await service.GetMessagesPageAsync(id, pageNumber, pageSize, true);

        if (!pageResult.IsSuccess) {
            return TypedResults.BadRequest(pageResult.Exceptions);
        }

        return TypedResults.Ok(pageResult.Value);
    }
}