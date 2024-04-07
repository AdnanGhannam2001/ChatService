using ChatService.Attributes;
using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;

namespace ChatService.Consumers;

[QueueConsumer]
internal sealed class FriendshipCreatedEventConsumer : IConsumer<FriendshipCreatedEvent> {
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public FriendshipCreatedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger) {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FriendshipCreatedEvent> context) {
        var result = await _service.AddChatAsync(context.Message.FriendId, context.Message.UserId);

        if (!result.IsSuccess) {
            _logger.LogCritical(
                "Something went wrong while attempting to create a group between: {FriendId} and {UserId}",
                context.Message.FriendId, context.Message.UserId);
        }
    }
}