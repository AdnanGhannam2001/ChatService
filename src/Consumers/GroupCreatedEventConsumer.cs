using MassTransit;
using PR2.Contracts.Events;

namespace ChatService.Consumers;

public sealed class GroupCreatedEventConsumer : IConsumer<GroupCreatedEvent> {
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public GroupCreatedEventConsumer(ILogger<GroupCreatedEventConsumer> logger) {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<GroupCreatedEvent> context) {
        // TODO: Handle Event
        _logger.LogInformation("Message From Queue {Message}", context.Message);

        return Task.CompletedTask;
    }
}