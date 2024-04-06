using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;
using PR2.Shared.Enums;

namespace ChatService.Consumers;

internal sealed class MemberRoleChangedEventConsumer : IConsumer<MemberRoleChangedEvent> {
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public MemberRoleChangedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger) {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MemberRoleChangedEvent> context) {
        Enum.TryParse<MemberRoleTypes>(context.Message.Role, true, out var role);
        var result = await _service.ChangeMemberRoleAsync(context.Message.GroupId, context.Message.MemberId, role);

        if (!result.IsSuccess) {
            _logger.LogCritical(
                "Something went wrong while attempting to change role for member with Id: {MemberId} in group with Id: {GroupId}",
                context.Message.MemberId, context.Message.GroupId);
        }
    }
}