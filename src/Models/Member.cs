namespace ChatService.Models;

public sealed class Member {
    #pragma warning disable CS8618
    private Member() { }
    #pragma warning restore CS8618

    public Member(string chatId, string userId) {
        ChatId = chatId;
        UserId = userId;
    }

    public string ChatId { get; init; }
    public string UserId { get; init; }
}