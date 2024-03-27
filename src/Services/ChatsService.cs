using System.Data;
using ChatService.Data;
using ChatService.Data.Sql;
using ChatService.Models;
using ChatService.Repositories.Repositories;
using Dapper;
using Npgsql;
using PR2.Shared.Common;
using PR2.Shared.Enums;

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
            // Logger
            return new ExceptionBase();
        }

        return chat;
    }

    public async Task<Result<Chat, ExceptionBase>> AddChatAsync(string user1Id, string user2Id, CancellationToken cancellationToken = default) {
        var chat = new Chat(user1Id, user2Id);

        if (!cancellationToken.IsCancellationRequested) {
            if (await _db.QueryAsync<string?>(ChatsQueries.Add, chat) is null) {
                return new ExceptionBase();
            }

            return chat;
        }

        return new ExceptionBase();
    }
}