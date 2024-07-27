using System.Data;
using ChatService.Data;
using ChatService.Data.Sql;
using ChatService.Exceptions;
using ChatService.Interfaces;
using ChatService.Models;
using Dapper;
using Npgsql;
using PR2.Shared.Common;
using PR2.Shared.Enums;
using PR2.Shared.Exceptions;

namespace ChatService.Services;

public sealed class ChatsService : IChatsService, IDisposable
{
    #region Fields & Constructor
    private readonly NpgsqlConnection _db;
    private readonly ILogger<ChatsService> _logger;

    public ChatsService(DapperDbConnection connection, ILogger<ChatsService> logger)
    {
        _logger = logger;
        _db = connection.CreateConnection();
    }
    #endregion

    #region CRUD Operations
    #region CREATE
    public async Task<Result<Chat>> AddGroupChatAsync(string groupId, string creatorId, CancellationToken cancellationToken = default)
    {
        var creator = new Member(groupId, creatorId, MemberRoleTypes.Admin);
        var chat = new Chat(groupId, [creator]);

        if (_db.FullState == ConnectionState.Closed) await _db.OpenAsync(cancellationToken);

        using var transaction = await _db.BeginTransactionAsync();

        try
        {
            await _db.QueryAsync(ChatsQueries.Add, chat, transaction);

            await _db.QueryAsync(MembersQueries.Add, creator, transaction);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new TransactionFailureException("Failed to create group chat");
        }

        return chat;
    }

    public async Task<Result<Chat>> AddChatAsync(string user1Id, string user2Id, CancellationToken cancellationToken = default)
    {
        var chat = new Chat(user1Id, user2Id);
        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        if (await _db.QueryAsync<string?>(ChatsQueries.Add, chat) is null)
        {
            await _db.QueryAsync(ChatsQueries.Activate);
        }

        return chat;
    }

    public async Task<Result<Member>> AddMemberAsync(string chatId,
        string memberId,
        MemberRoleTypes role,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        var member = new Member(chatId, memberId, role);

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        try
        {
            await _db.QueryAsync(MembersQueries.Add, member);
        }
        catch (Exception)
        {
            return new DuplicatedRecordException("Member already exists in chat");
        }

        return member;
    }

    public async Task<Result<Message>> SendMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(message.ChatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        if (!chatResult.Value.IsActive)
        {
            return new ChatInactiveException();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        if (_db.FullState == ConnectionState.Closed) await _db.OpenAsync(cancellationToken);

        using var transaction = _db.BeginTransaction();
        try
        {
            await _db.QueryFirstAsync<string>(MessagesQueries.Add, message, transaction);
            await _db.QueryAsync(ChatsQueries.NewMessage, new
            {
                Id = message.ChatId,
                LastMessageAt = DateTime.UtcNow,
            }, transaction);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new TransactionFailureException("Failed to send a message to chat");
        }

        return message;
    }
    #endregion

    #region READ
    public async Task<Page<Chat>> GetChatsPageAsync(string userId, int pageNumber, int pageSize, bool desc = false)
    {
        var items = await _db.QueryAsync<Chat>(desc ? ChatsQueries.ListDesc : ChatsQueries.ListAsc,
            new { PageSize = pageSize, PageNumber = pageNumber, UserId = userId });

        var total = await _db.QueryFirstAsync<int>(ChatsQueries.Count, new { UserId = userId });

        return new(items, total);
    }

    public async Task<Result<Chat>> GetChatByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = id });

        if (chat is null)
        {
            return new RecordNotFoundException($"Chat with Id: {id} is not found");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        return chat;
    }

    public async Task<Result<Member>> GetMemberAsync(string chatId, string memberId, CancellationToken cancellationToken = default)
    {
        var member = await _db.QueryFirstOrDefaultAsync<Member>(MembersQueries.Get, new { ChatId = chatId, UserId = memberId });

        if (member is null)
        {
            return new RecordNotFoundException($"Member is not found in chat with Id: {chatId}");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        return member;
    }

    public async Task<Result<Page<Message>>> GetMessagesPageAsync(string chatId,
        int pageNumber,
        int pageSize,
        bool desc = false,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        var items = await _db.QueryAsync<Message>(desc ? MessagesQueries.ListDesc : MessagesQueries.ListAsc,
            new { ChatId = chatId, PageNumber = pageNumber, PageSize = pageSize });

        var total = await _db.QueryFirstAsync<int>(MessagesQueries.Count, new { ChatId = chatId });

        return new Page<Message>(items, total);
    }

    public async Task<Result<Message>> GetMessageByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var message = await _db.QueryFirstOrDefaultAsync<Message>(MessagesQueries.GetById,
            new { Id = id });

        if (message is null)
        {
            return new RecordNotFoundException("Message is not found");
        }

        return message;
    }
    #endregion


    #region UPDATE
    public async Task<Result<int>> ChangeMemberRoleAsync(string chatId,
        string memberId,
        MemberRoleTypes role,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        await _db.QueryAsync<int>(MembersQueries.ChangeRole, new
        {
            ChatId = chatId,
            UserId = memberId,
            Role = role
        });

        return 1;
    }

    public async Task<Result<int>> UpdateMessageAsync(string chatId,
        string messageId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        if (!chatResult.Value.IsActive)
        {
            return new ChatInactiveException();
        }

        var messageResult = await GetMessageByIdAsync(messageId, cancellationToken);

        if (!messageResult.IsSuccess)
        {
            return messageResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        if (_db.FullState == ConnectionState.Closed) await _db.OpenAsync(cancellationToken);

        using var transaction = _db.BeginTransaction();

        try
        {
            await _db.QueryAsync(MessagesQueries.Update,
                new { Id = messageId, Content = content, LastUpdateAt = DateTime.UtcNow });
            await _db.QueryAsync(ChatsQueries.NewMessage, new
            {
                Id = messageResult.Value.ChatId,
                LastMessageAt = DateTime.UtcNow,
            }, transaction);

            await transaction.CommitAsync(cancellationToken);

            return 1;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new TransactionFailureException("Failed to update message in chat");
        }
    }
    #endregion

    #region DELETE
    public async Task<Result<int>> DeleteChatAsync(string id, CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(id, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        await _db.QueryAsync(ChatsQueries.SoftDelete, new { Id = id });

        return 1;
    }

    public async Task<Result<int>> DeleteMemberAsync(string chatId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        await _db.QueryAsync(MembersQueries.Delete,
            new { ChatId = chatId, UserId = memberId });

        return 1;
    }

    public async Task<Result<int>> DeleteMessageAsync(string chatId, string messageId, CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess)
        {
            return chatResult.Exceptions;
        }

        if (!chatResult.Value.IsActive)
        {
            return new ChatInactiveException();
        }

        var messageResult = await GetMessageByIdAsync(messageId, cancellationToken);

        if (!messageResult.IsSuccess)
        {
            return messageResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationCancelledException("Operation just got cancelled");
        }

        await _db.QueryAsync(MessagesQueries.Delete, new { Id = messageId });

        return 1;
    }
    #endregion
    #endregion

    #region Static
    public static bool HasMinimalRole(MemberRoleTypes memberRole, MemberRoleTypes minimalRole) => memberRole <= minimalRole;
    #endregion

    public void Dispose() => _db.Dispose();
}