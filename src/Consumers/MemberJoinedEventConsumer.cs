using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;
using PR2.Shared.Enums;

namespace ChatService.Consumers;

internal sealed class MemberJoinedEventConsumer : IConsumer<MemberJoinedEvent>
{
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public MemberJoinedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger) {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MemberJoinedEvent> context) {
        Enum.TryParse<MemberRoleTypes>(context.Message.Role, true, out var role);
        var result = await _service.AddMemberAsync(context.Message.GroupId, context.Message.MemberId, role);

        if (!result.IsSuccess) {
            _logger.LogCritical(
                "Something went wrong while attempting to add member with Id: {MemberId} to group with Id: {GroupId}",
                context.Message.MemberId, context.Message.GroupId);
        }
    }
}