using System.Security.Claims;
using ChatService.Interfaces;
using ChatService.Policies.Requirements;
using ChatService.Services;
using Microsoft.AspNetCore.Authorization;

namespace ChatService.Policies.Handlers;

internal sealed class MembershipHandler : AuthorizationHandler<MembershipRequirement>
{
    private readonly HttpContext _httpContext;
    private readonly IChatsService _service;

    public MembershipHandler(IHttpContextAccessor httpContextAccessor, IChatsService service)
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _service = service;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MembershipRequirement requirement)
    {
        var routeData = _httpContext.GetRouteData().Values;
        var chatId = routeData["id"] as string;
        var userId = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        var memberResult = await _service.GetMemberAsync(chatId!, userId);

        if (memberResult.IsSuccess)
        {
            if (ChatsService.HasMinimalRole(memberResult.Value.Role, requirement.MinimalRole))
            {
                context.Succeed(requirement);
                return;
            }

            var messageId = routeData["messageId"] as string;

            var messageResult = await _service.GetMessageByIdAsync(messageId!);

            if (messageResult.IsSuccess && messageResult.Value.SenderId == userId)
            {
                context.Succeed(requirement);
            }
        }

        context.Fail();
    }
}