using ChatService.Data.Sql;
using ChatService.Models;
using Dapper;
using Npgsql;
using PR2.Shared.Common;
using PR2.Shared.Exceptions;

namespace ChatService.Services;

public sealed class MessagesService : IDisposable
{
    #region Fields & Constructor
    private readonly NpgsqlConnection _db;
    private readonly ILogger<MessagesService> _logger;

    public MessagesService(NpgsqlConnection db, ILogger<MessagesService> logger)
    {
        _db = db;
        _logger = logger;
    }
    #endregion

    #region CRUD Operations
    #region CREATE
    public async Task<Result<Message>> SendMessageAsync(Message message, CancellationToken cancellationToken = default) {
        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        // TODO: Handle Failure
        await _db.QueryFirstAsync<string>(MessagesQueries.Add, message);

        return message;
    }
    #endregion

    #region READ
    public async Task<Page<Message>> GetMessagesPageAsync(int pageNumber, int pageSize, bool desc = false) {
        var items = await _db.QueryAsync<Message>(MessagesQueries.List,
            new { PageNumber = pageNumber, PageSize = pageSize });

        var total = await _db.QueryFirstAsync<int>(MessagesQueries.Count);

        return new(items, total);
    }
    #endregion

    #region UPDATE
    public async Task<Result<int>> UpdateMessageAsync(string id, string content, CancellationToken cancellationToken = default) {
        var message = await _db.QueryFirstOrDefaultAsync(MessagesQueries.GetById,
            new { Id = id });

        if (message is null) {
            return new RecordNotFoundException("Message is not found");
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var affected = await _db.QueryFirstAsync<int>(MessagesQueries.Update,
            new { Id = id, Content = content });

        return affected;
    }
    #endregion

    #region DELETE
    public async Task<Result<int>> DeleteMessageAsync(string id, CancellationToken cancellationToken = default) {
        var message = await _db.QueryFirstOrDefaultAsync(MessagesQueries.GetById,
            new { Id = id });

        if (message is null) {
            return new RecordNotFoundException("Message is not found");
        }

        if (cancellationToken.IsCancellationRequested) {
            return new OperationCancelledException("Operation just got cancelled");
        }

        var affected = await _db.QueryFirstAsync<int>(MessagesQueries.Delete, new { Id = id });

        return affected;
    }
    #endregion
    #endregion

    public void Dispose() => _db.Dispose();
}