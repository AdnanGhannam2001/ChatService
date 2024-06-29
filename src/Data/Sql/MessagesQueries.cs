namespace ChatService.Data.Sql;

public static class MessagesQueries
{
    private const string _table = "\"Messages\"";

    public const string GetById = $"""
        SELECT *
        FROM {_table}
        WHERE "Id" = @Id;
    """;

    public const string Add = $"""
        INSERT INTO {_table} ("Id", "ChatId", "SenderId", "Content", "SentAt", "LastUpdateAt")
        VALUES (@Id, @ChatId, @SenderId, @Content, @SentAt, @LastUpdateAt)
        RETURNING "Id";
    """;

    public const string Count = $"""
        SELECT COUNT(*)
        FROM {_table}
        WHERE "ChatId" = @ChatId;
    """;

    public const string ListAsc = $"""
        SELECT *
        FROM {_table}
        WHERE "ChatId" = @ChatId
        ORDER BY "SentAt" ASC
        LIMIT @PageSize
        OFFSET @PageNumber * @PageSize;
    """;

    public const string ListDesc = $"""
        SELECT *
        FROM {_table}
        WHERE "ChatId" = @ChatId
        ORDER BY "SentAt" DESC
        LIMIT @PageSize
        OFFSET @PageNumber * @PageSize;
    """;

    public const string Update = $"""
        UPDATE {_table}
        SET
            "Content" = @Content,
            "LastUpdateAt" = @LastUpdateAt
        WHERE "Id" = @Id;
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "Id" = @Id;
    """;
}