using NanoidDotNet;

namespace ChatService.Models;

public sealed class Message {
    #pragma warning disable CS8618
    private Message() { }
    #pragma warning restore CS8618

    public Message(string chatId, string senderId, string content) {
        Id = Nanoid.Generate(size: 15);
        SenderId = senderId;
        ChatId = chatId;
        Content = content;
        SentAt = LastUpdateAt = DateTime.UtcNow;
    }

    public string Id { get; init; }
    public string SenderId { get; init; }
    public string ChatId { get; init; }
    public DateTime SentAt { get; init; }
    public string Content { get; private set; }
    public DateTime LastUpdateAt { get; private set; }
}