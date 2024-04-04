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
        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = chatId });

        if (chat is null) {
            return new RecordNotFoundException($"Chat with Id: {chatId} is not found");
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

    #endregion

    #region READ
    public async Task<Page<Chat>> GetChatsPageAsync(int pageNumber, int pageSize, bool desc = false) {
        var items = await _db.QueryAsync<Chat>(ChatsQueries.List,
            new { PageSize = pageSize, PageNumber = pageNumber });

        var total = await _db.QueryFirstAsync<int>(ChatsQueries.Count);

        return new(items, total);
    }
    #endregion

    #region UPDATE
    public async Task<Result<int>> ChangeMemberRoleAsync(string chatId,
        string memberId,
        MemberRoleTypes role,
        CancellationToken cancellationToken = default)
    {
        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = chatId });

        if (chat is null) {
            return new RecordNotFoundException($"Chat with Id: {chatId} is not found");
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
    #endregion
    
    #region DELETE
    public async Task<Result<int>> DeleteChatAsync(string id, CancellationToken cancellationToken = default) {
        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = id });

        if (chat is null) {
            return new RecordNotFoundException($"Chat with Id: {id} is not found");
        }
        
        var affected = await _db.QueryFirstAsync<int>(ChatsQueries.SoftDelete, new { Id = id });

        return affected;
    }

    public async Task<Result<int>> DeleteMemberAsync(string chatId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = chatId });

        if (chat is null) {
            return new RecordNotFoundException($"Chat with Id: {chatId} is not found");
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }
        var affected = await _db.QueryFirstAsync<int>(MembersQueries.Delete, new { ChatId = chatId, MemberId = memberId });

        return affected;
    }

    #endregion
    #endregion

    public void Dispose() => _db.Dispose();
}