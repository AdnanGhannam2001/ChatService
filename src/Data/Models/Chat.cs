namespace ChatService.Data.Models;

public sealed class Chat
{
#pragma warning disable CS8618
    private Chat() { }
#pragma warning restore CS8618

    public Chat(string user1Id, string user2Id)
    {
        Id = $"{user1Id}|{user2Id}";
        IsGroup = false;
        IsActive = true;
        Messages = [];
        Members = null;
    }

    public Chat(string groupId, IEnumerable<Member> members)
    {
        Id = groupId;
        IsGroup = true;
        IsActive = true;
        Members = members.ToList();
        Messages = [];
    }

    public string Id { get; init; }
    // TODO Make this init
    public bool IsGroup { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public ICollection<Message> Messages { get; private set; }
    public ICollection<Member>? Members { get; private set; }
}