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

public sealed class ChatsService {
    private readonly NpgsqlConnection _db;
    private readonly ILogger<ChatsService> _logger;

    public ChatsService(DapperDbConnection connection, ILogger<ChatsService> logger) {
        _logger = logger;
        _db = connection.CreateConnection();
    }

    public async Task<Result<Chat, ExceptionBase>> AddGroupChatAsync(string groupId, string creatorId, CancellationToken cancellationToken = default) {
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

    public async Task<Result<Chat, ExceptionBase>> AddChatAsync(string user1Id, string user2Id, CancellationToken cancellationToken = default) {
        var chat = new Chat(user1Id, user2Id);

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        if (await _db.QueryAsync<string?>(ChatsQueries.Add, chat) is null) {
            return new DuplicatedRecordException("Chat already exists");
        }

        return chat;
    }

    public async Task<Result<int, ExceptionBase>> DeleteChatAsync(string id, CancellationToken cancellationToken = default) {
        var chat = await _db.QueryFirstOrDefaultAsync<Chat?>(ChatsQueries.GetById, new { Id = id });

        if (chat is null) {
            return new RecordNotFoundException($"Chat with Id: {id} is not found");
        }
        
        var affected = await _db.QueryFirstAsync<int>(ChatsQueries.SoftDelete, new { Id = id });

        return affected;
    }

    public async Task<Result<Member, ExceptionBase>> AddMemberAsync(string chatId,
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

    public async Task<Result<int, ExceptionBase>> DeleteMemberAsync(string chatId,
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

    public async Task<Result<int, ExceptionBase>> ChangeMemberRoleAsync(string chatId,
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
}