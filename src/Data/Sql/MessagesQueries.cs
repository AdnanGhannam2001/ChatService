namespace ChatService.Data.Sql;

public static class MessagesQueries {
    private const string _table = "\"Messages\"";

    public const string GetById = $"""
        SELECT *
        FROM {_table}
        WHERE "Id" = @Id;
    """;

    public const string Add = $"""
        INSERT INTO {_table} ("Id", "ChatId", "SenderId", "Content")
        VALUES (@Id, @ChatId, @SenderId, @Content)
        RETURNING "Id";
    """;

    public const string Count = $"""
        SELECT COUNT(*)
        FROM {_table};
    """;

    public const string ListAsc = $"""
        SELECT *
        FROM {_table}
        ORDER BY "SentAt" ASC
        LIMIT @PageSize
        OFFSET @PageNumber;
    """;

    public const string ListDesc = $"""
        SELECT *
        FROM {_table}
        ORDER BY "SentAt" DESC
        LIMIT @PageSize
        OFFSET @PageNumber;
    """;

    public const string Update = $"""
        UPDATE {_table}
        SET "Content" = @Content
        WHERE "Id" = @Id;
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "Id" = @Id;
    """;
}