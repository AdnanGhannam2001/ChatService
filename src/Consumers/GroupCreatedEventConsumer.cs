using ChatService.Attributes;
using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;

namespace ChatService.Consumers;

[QueueConsumer]
internal sealed class GroupCreatedEventConsumer : IConsumer<GroupCreatedEvent> {
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public GroupCreatedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger) {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GroupCreatedEvent> context) {
        var result = await _service.AddGroupChatAsync(context.Message.GroupId, context.Message.CreatorId);

        if (!result.IsSuccess) {
            _logger.LogCritical(
                "Something went wrong while attempting to create a group with Id: {GroupId} and creator Id: {CreatorId}",
                context.Message.GroupId, context.Message.CreatorId);
        }
    }
}