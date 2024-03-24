using System.Linq.Expressions;
using ChatService.Data;
using ChatService.Data.Sql;
using ChatService.Models;
using Dapper;
using Npgsql;
using PR2.Shared.Common;
using PR2.Shared.Enums;
using PR2.Shared.Interfaces;

namespace ChatService.Repositories.Repositories;

public sealed class ChatsRepository
    : IReadRepository<Chat>, IWriteRepository<Chat, NpgsqlTransaction>, IDisposable
{
    private readonly NpgsqlConnection _db;

    public ChatsRepository(DapperDbConnection connection) => _db = connection.CreateConnection();

    #region Implementation
    public async Task<Chat> AddAsync(Chat entity, CancellationToken cancellationToken = default) {
        await _db.OpenAsync(cancellationToken);
        using var transaction = await _db.BeginTransactionAsync(cancellationToken);

        try {
            await _db.QueryAsync(ChatsQueries.Add, entity, transaction);

            if (entity.Members is not null) {
                foreach (var member in entity.Members) {
                    await _db.QueryAsync(MembersQueries.Add, member, transaction);
                }
            }
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception) {
            await transaction.RollbackAsync();
        }

        return entity;
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => await _db.QueryFirstAsync<int>(ChatsQueries.Count) > 0;

    public async Task<int> CountAsync(CancellationToken cancellationToken = default) 
        => await _db.QueryFirstAsync<int>(ChatsQueries.Count);

    public async Task DeleteAsync(Chat entity, CancellationToken cancellationToken = default) 
        => await _db.QueryAsync(ChatsQueries.Delete, new { entity.Id });

    public async Task<Chat?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => await _db.QueryFirstOrDefaultAsync(ChatsQueries.GetById);

    public async Task<Page<Chat>> GetPageAsync<TKey>(int pageNumber, int pageSize, Func<Chat, TKey>? keySelector = null, bool desc = false, CancellationToken cancellationToken = default) {
        var count = await CountAsync(cancellationToken);
        var items = await _db.QueryAsync<Chat>(ChatsQueries.GetPage, new { PageNumber = pageNumber, PageSize = pageSize });

        return new(items, count);
    }

    public async Task<List<Chat>> ListAsync(CancellationToken cancellationToken = default)
        => (await _db.QueryAsync<Chat>(ChatsQueries.List)).ToList();

    public Task UpdateAsync(Chat entity, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Chats are immutable");
    #endregion

    public async Task<int> AddMemberAsync(string chatId, string userId, MemberRoleTypes role, CancellationToken cancellationToken = default) {
        var chat = await GetByIdAsync(chatId, cancellationToken);

        if (chat is null) {
            return 0;
        }

        var member = new Member(chatId, userId, role);
        return await _db.QueryFirstAsync<int>(MembersQueries.Add, member);
    }

    public async Task<int> DeleteMemberAsync(string chatId, string userId, CancellationToken cancellationToken = default) {
        var chat = await GetByIdAsync(chatId, cancellationToken);

        if (chat is null) {
            return 0;
        }

        var member = new Member(chatId, userId);
        return await _db.QueryFirstAsync<int>(MembersQueries.Delete, member);
    }

    #region NotImplemented
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("This method is not implemented currently for 'Dapper'");

    public Task<bool> AnyAsync(Expression<Func<Chat, bool>> predicate, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("This method is not implemented currently for 'Dapper'");

    public Task<NpgsqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("This method is not implemented currently for 'Dapper'");

    public Task<int> CountAsync(Expression<Func<Chat, bool>> predicate, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("This method is not implemented currently for 'Dapper'");

    public Task DeleteRangeAsync(IEnumerable<Chat> entities, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("This method is not implemented currently for 'Dapper'");
    #endregion

    public void Dispose() => _db.Dispose();
}