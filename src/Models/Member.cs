namespace ChatService.Models;

public sealed class Member {
    #pragma warning disable CS8618
    private Member() { }
    #pragma warning restore CS8618

    public string ChatId { get; init; }
    public string UserId { get; init; }
}