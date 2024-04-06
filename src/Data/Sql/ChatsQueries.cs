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

    // TODO Get User Chats too (not only group chats)
    public const string ListAsc = $"""
        SELECT *
        FROM {_table}
        JOIN "Members" ON {_table}."Id" = "Members"."ChatId"
        WHERE "Members"."UserId" = @UserId
        ORDER BY "LastMessageAt" ASC
        LIMIT @PageSize
        OFFSET @PageNumber;
    """;

    public const string ListDesc = $"""
        SELECT *
        FROM {_table}
        JOIN "Members" ON {_table}."Id" = "Members"."ChatId"
        WHERE "Members"."UserId" = @UserId
        ORDER BY "LastMessageAt" DESC
        LIMIT @PageSize
        OFFSET @PageNumber;
    """;

    public const string NewMessage = $"""
        UPDATE {_table}
        SET "LastMessageAt" = @LastMessageAt
        WHERE "Id" = @Id;
    """;

    public const string SoftDelete = $"""
        UPDATE {_table}
        SET "IsActive" = FALSE
        WHERE "Id" = @Id;
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "Id" = @Id
        RETURNING "_computed";
    """;
}