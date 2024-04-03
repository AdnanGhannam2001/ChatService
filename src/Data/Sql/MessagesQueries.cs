namespace ChatService.Data.Sql;

public static class MessagesQueries {
    private const string _table = "\"Messages\"";

    public const string GetById = $"""
        SELECT *
        FROM {_table}
        WHERE "Id" = @Id;
    """;

    public const string Add = $"""
        INSERT INTO {_table} ("ChatId", "SenderId", "Content")
        VALUES (@ChatId, @SenderId, @Content)
        RETURNING "Id";
    """;

    public const string Count = $"""
        SELECT COUNT(*)
        FROM {_table};
    """;

    public const string List = $"""
        SELECT *
        FROM {_table}
        LIMIT @PageSize
        OFFSET @PageNumber
    """;

    public const string Update = $"""
        UPDATE {_table}
        SET "Content" = @Content
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;
}