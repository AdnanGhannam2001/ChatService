using ChatService.Models;
using MassTransit;
using PR2.Contracts.Events;
using PR2.Shared.Enums;

namespace ChatService.Consumers;

public sealed class GroupCreatedEventConsumer : IConsumer<GroupCreatedEvent> {
    private readonly ILogger<GroupCreatedEventConsumer> _logger;

    public GroupCreatedEventConsumer(ILogger<GroupCreatedEventConsumer> logger) {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GroupCreatedEvent> context) {
        _logger.LogInformation("Received: {Message}", context.Message);

        var creator = new Member(context.Message.GroupId, context.Message.CreatorId, MemberRoleTypes.Admin);
        var chat = new Chat(context.Message.GroupId, [creator]);
        // await _repo.AddAsync(chat);
    }
}