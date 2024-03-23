namespace ChatService.Models;

public sealed class Chat {
    #pragma warning disable CS8618
    private Chat() { }
    #pragma warning restore CS8618

    public Chat(string user1Id, string user2Id) {
        Id = $"{user1Id}_{user2Id}";
        IsGroup = false;
        Messages = [];
        Members = null;
    }

    public Chat(string groupId, IEnumerable<Member> members) {
        Id = groupId;
        IsGroup = true;
        Members = members.ToList();
        Messages = [];
    }

    public string Id { get; init; }
    public bool IsGroup { get; private set; }
    public ICollection<Message> Messages { get; private set; }
    public ICollection<Member>? Members { get; private set; }
}