using System.Data;
using ChatService.Data;
using ChatService.Data.Sql;
using ChatService.Models;
using Dapper;
using Npgsql;
using PR2.Shared.Common;
using PR2.Shared.Enums;
using PR2.Shared.Exceptions;

namespace ChatService.Services;

public sealed class ChatsService : IDisposable {
    #region Fields & Constructor
    private readonly NpgsqlConnection _db;
    private readonly ILogger<ChatsService> _logger;

    public ChatsService(DapperDbConnection connection, ILogger<ChatsService> logger) {
        _logger = logger;
        _db = connection.CreateConnection();
    }
    #endregion

    #region CRUD Operations
    #region CREATE
    public async Task<Result<Chat>> AddGroupChatAsync(string groupId, string creatorId, CancellationToken cancellationToken = default) {
        var creator = new Member(groupId, creatorId, MemberRoleTypes.Admin);
        var chat = new Chat(groupId, [creator]);

        if (_db.FullState == ConnectionState.Closed) await _db.OpenAsync(cancellationToken);

        using var transaction = await _db.BeginTransactionAsync();

        try {
            await _db.QueryAsync(ChatsQueries.Add, chat, transaction);

            await _db.QueryAsync(MembersQueries.Add, creator, transaction);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception) {
            await transaction.RollbackAsync();
            return new TransactionFailureException("Failed to create group chat");
        }

        return chat;
    }

    public async Task<Result<Chat>> AddChatAsync(string user1Id, string user2Id, CancellationToken cancellationToken = default) {
        var chat = new Chat(user1Id, user2Id);
        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        if (await _db.QueryAsync<string?>(ChatsQueries.Add, chat) is null) {
            return new DuplicatedRecordException("Chat already exists");
        }

        return chat;
    }

    public async Task<Result<Member>> AddMemberAsync(string chatId,
        string memberId,
        MemberRoleTypes role,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }
        
        var member = new Member(chatId, memberId, role);

        try {
            await _db.QueryAsync(MembersQueries.Add, member);
        }
        catch (Exception) {
            return new DuplicatedRecordException("Member already exists in chat");
        }

        return member;
    }

    public async Task<Result<Message>> SendMessageAsync(Message message, CancellationToken cancellationToken = default) {
        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var chatResult = await GetChatByIdAsync(message.ChatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        // TODO: Handle Failure
        await _db.QueryFirstAsync<string>(MessagesQueries.Add, message);

        return message;
    }
    #endregion

    #region READ
    public async Task<Page<Chat>> GetChatsPageAsync(int pageNumber, int pageSize, bool desc = false) {
        var items = await _db.QueryAsync<Chat>(ChatsQueries.List,
            new { PageSize = pageSize, PageNumber = pageNumber });

        var total = await _db.QueryFirstAsync<int>(ChatsQueries.Count);

        return new(items, total);
    }

    public async Task<Result<Chat>> GetChatByIdAsync(string id, CancellationToken cancellationToken = default) {
        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = id });

        if (chat is null) {
            return new RecordNotFoundException($"Chat with Id: {id} is not found");
        }

        return chat;
    }

    public async Task<Result<Page<Message>>> GetMessagesPageAsync(string chatId,
        int pageNumber,
        int pageSize,
        bool desc = false,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        var items = await _db.QueryAsync<Message>(MessagesQueries.List,
            new { PageNumber = pageNumber, PageSize = pageSize });

        var total = await _db.QueryFirstAsync<int>(MessagesQueries.Count);

        return new Page<Message>(items, total);
    }
    #endregion

    #region UPDATE
    public async Task<Result<int>> ChangeMemberRoleAsync(string chatId,
        string memberId,
        MemberRoleTypes role,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }
        
        var affected = await _db.QueryFirstAsync<int>(MembersQueries.ChangeRole, new {
            ChatId = chatId,
            MemberId = memberId,
            Role = role
        });

        return affected;
    }

    public async Task<Result<int>> UpdateMessageAsync(string chatId, string messageId, string content, CancellationToken cancellationToken = default) {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        var message = await _db.QueryFirstOrDefaultAsync(MessagesQueries.GetById,
            new { Id = messageId });

        if (message is null) {
            return new RecordNotFoundException("Message is not found");
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var affected = await _db.QueryFirstAsync<int>(MessagesQueries.Update,
            new { Id = messageId, Content = content });

        return affected;
    }
    #endregion
    
    #region DELETE
    public async Task<Result<int>> DeleteChatAsync(string id, CancellationToken cancellationToken = default) {
        var chatResult = await GetChatByIdAsync(id, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }
        
        var affected = await _db.QueryFirstAsync<int>(ChatsQueries.SoftDelete, new { Id = id });

        return affected;
    }

    public async Task<Result<int>> DeleteMemberAsync(string chatId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }
        var affected = await _db.QueryFirstAsync<int>(MembersQueries.Delete, new { ChatId = chatId, MemberId = memberId });

        return affected;
    }

    public async Task<Result<int>> DeleteMessageAsync(string chatId, string messageId, CancellationToken cancellationToken = default) {
        var chatResult = await GetChatByIdAsync(chatId, cancellationToken);

        if (!chatResult.IsSuccess) {
            return chatResult.Exceptions;
        }

        var message = await _db.QueryFirstOrDefaultAsync(MessagesQueries.GetById,
            new { Id = messageId });

        if (message is null) {
            return new RecordNotFoundException("Message is not found");
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var affected = await _db.QueryFirstAsync<int>(MessagesQueries.Delete, new { Id = messageId });

        return affected;
    }
    #endregion
    #endregion

    public void Dispose() => _db.Dispose();
}