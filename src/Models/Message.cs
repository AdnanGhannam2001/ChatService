namespace ChatService.Models;

public sealed class Message {
    #pragma warning disable CS8618
    private Message() { }
    #pragma warning restore CS8618

    public string Id { get; init; }
    public string SenderId { get; init; }
    public string ChatId { get; init; }
    public DateTime SentAt { get; init; }
    public string Content { get; set; }
    public DateTime LastUpdateAt { get; set; }
}