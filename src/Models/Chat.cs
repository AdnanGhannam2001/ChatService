namespace ChatService.Models;

public sealed class Chat {
    #pragma warning disable CS8618
    private Chat() { }
    #pragma warning restore CS8618

    public string Id { get; init; }
    public string IsGroup { get; set; }
    public ICollection<Message> Messages { get; set; }
    public ICollection<Member> Members { get; set; }
}