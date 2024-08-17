using ChatService.Data.Models;

namespace ChatService.Interfaces;

public interface IChatClient
{
    Task MessageSent(Message message);
    Task MessageUpdated(string messageId, string content);
    Task MessageDeleted(string id);
}