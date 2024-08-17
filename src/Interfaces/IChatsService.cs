using ChatService.Data.Models;
using PR2.Shared.Common;
using PR2.Shared.Enums;

namespace ChatService.Interfaces;

public interface IChatsService
{
    Task<Result<Chat>> AddGroupChatAsync(string groupId, string creatorId, CancellationToken cancellationToken = default);
    Task<Result<Chat>> AddChatAsync(string user1Id, string user2Id, CancellationToken cancellationToken = default);
    Task<Result<Member>> AddMemberAsync(string chatId, string memberId, MemberRoleTypes role, CancellationToken cancellationToken = default);
    Task<Result<Message>> SendMessageAsync(Message message, CancellationToken cancellationToken = default);
    Task<Page<Chat>> GetChatsPageAsync(string userId, int pageNumber, int pageSize, bool desc = false);
    Task<Result<Chat>> GetChatByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<Member>> GetMemberAsync(string chatId, string memberId, CancellationToken cancellationToken = default);
    Task<Result<Page<Message>>> GetMessagesPageAsync(string chatId, int pageNumber, int pageSize, bool desc = false, CancellationToken cancellationToken = default);
    Task<Result<Message>> GetMessageByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<int>> ChangeMemberRoleAsync(string chatId, string memberId, MemberRoleTypes role, CancellationToken cancellationToken = default);
    Task<Result<int>> UpdateMessageAsync(string chatId, string messageId, string content, CancellationToken cancellationToken = default);
    Task<Result<int>> DeleteChatAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<int>> DeleteMemberAsync(string chatId, string memberId, CancellationToken cancellationToken = default);
    Task<Result<int>> DeleteMessageAsync(string chatId, string messageId, CancellationToken cancellationToken = default);
}