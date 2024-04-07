using ChatService.Attributes;
using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;

namespace  ChatService.Consumers;

[QueueConsumer]
internal sealed class MemberLeavedEventConsumer : IConsumer<MemberLeavedEvent> {
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public MemberLeavedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger) {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MemberLeavedEvent> context) {
        var result = await _service.DeleteMemberAsync(context.Message.GroupId, context.Message.MemberId);

        if (!result.IsSuccess) {
            _logger.LogCritical(
                "Something went wrong while attempting to remove member with Id: {MemberId} from group with Id: {GroupId}",
                context.Message.MemberId, context.Message.GroupId);
        }
    }
}
