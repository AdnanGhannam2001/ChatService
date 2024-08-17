using PR2.Shared.Enums;

namespace ChatService.Data.Models;

public sealed class Member
{
#pragma warning disable CS8618
    private Member() { }
#pragma warning restore CS8618

    public Member(string chatId, string userId, MemberRoleTypes role = MemberRoleTypes.Normal)
    {
        ChatId = chatId;
        UserId = userId;
        Role = role;
    }

    public string ChatId { get; init; }
    public string UserId { get; init; }
    public MemberRoleTypes Role { get; private set; }
}