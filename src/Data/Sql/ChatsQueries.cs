namespace ChatService.Data.Sql;

public static class ChatsQueries {
    private const string _table = "\"Chats\"";
    public const string Add = $"""
        INSERT INTO {_table} ("Id", "IsGroup")
        VALUES (@Id, @IsGroup)
        RETURNING "Id";
    """;

    public const string Count = $"""
        SELECT COUNT(*)
        FROM {_table};
    """;

    public const string GetById = $"""
        SELECT *
        FROM {_table}
        WHERE "Id" = @Id;
    """;

    public const string ListAsc = $"""
        SELECT *
        FROM {_table}
        ORDER BY "LastMessageAt" ASC
        LIMIT @PageSize
        OFFSET @PageNumber;
    """;

    public const string ListDesc = $"""
        SELECT *
        FROM {_table}
        ORDER BY "LastMessageAt" DESC
        LIMIT @PageSize
        OFFSET @PageNumber;
    """;

    // TODO
    public const string Update = $"""
        UPDATE {_table}
        SET 
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;

    public const string NewMessage = $"""
        UPDATE {_table}
        SET "LastMessageAt" = @LastMessageAt
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;

    public const string SoftDelete = $"""
        UPDATE {_table}
        SET "IsActive" = FALSE
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;
}