using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;

namespace ChatService.Consumers;

internal sealed class FriendshipDeletedEventConsumer : IConsumer<FriendshipDeletedEvent> {
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public FriendshipDeletedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger) {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FriendshipDeletedEvent> context) {
        // TODO: fix this bc this may cause a problem
        var id = $"{context.Message.FriendId}_{context.Message.UserId}";
        var result = await _service.DeleteChatAsync(id);

        if (!result.IsSuccess) {
            _logger.LogCritical("Something went wrong while attempting to remove a group with Id: {id}", id);
        }
    }
}