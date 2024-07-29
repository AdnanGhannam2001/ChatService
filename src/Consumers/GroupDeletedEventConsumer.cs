using ChatService.Interfaces;
using MassTransit;
using PR2.Contracts.Events;
using PR2.RabbitMQ.Attributes;

namespace ChatService.Consumers;

[QueueConsumer]
internal sealed class GroupDeletedEventConsumer : IConsumer<GroupDeletedEvent>
{
    private readonly IChatsService _service;
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public GroupDeletedEventConsumer(IChatsService service, ILogger<GroupCreatedEventConsumer> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GroupDeletedEvent> context)
    {
        var result = await _service.DeleteChatAsync(context.Message.GroupId);

        if (!result.IsSuccess)
        {
            _logger.LogCritical("Something went wrong while attempting to delete a group with Id: {GroupId}",
                context.Message.GroupId);
        }
    }
}