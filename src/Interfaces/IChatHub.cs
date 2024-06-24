using ChatService.Models;

namespace ChatService.Interfaces;

public interface IChatClient
{
    Task SendConnectionId(string connectionId);

    Task MessageSent(Message message);
    Task MessageUpdated(string messageId, string content);
    Task MessageDeleted(string id);
}