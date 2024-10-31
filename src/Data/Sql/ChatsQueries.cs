namespace ChatService.Data.Sql;

public static class ChatsQueries
{
    private const string _table = "\"Chats\"";

    public const string Add = $"""
        INSERT INTO {_table} ("Id", "IsGroup")
        VALUES (@Id, @IsGroup)
        RETURNING "Id";
    """;

    public const string Count = $"""
        SELECT
            (
                SELECT COUNT(*)
                FROM "Members" 
                WHERE "Members"."UserId" = @UserId
            )
            +
            (
                SELECT COUNT(*)
                FROM {_table} t
                WHERE "Id" LIKE CONCAT('%', @UserId, '%')
            );
    """;

    public const string GetById = $"""
        SELECT *
        FROM {_table}
        WHERE "Id" = @Id;
    """;

    public const string ListAsc = $"""
        SELECT *
        FROM {_table} t
        WHERE 0 < (
                SELECT COUNT(*)
                FROM "Members" 
                WHERE t."Id" = "Members"."ChatId" AND "Members"."UserId" = @UserId
            )
            OR 
            "Id" LIKE CONCAT('%', @UserId, '%')
        ORDER BY "LastMessageAt" ASC
        LIMIT @PageSize
        OFFSET @PageNumber * @PageSize;
    """;

    public const string ListDesc = $"""
        SELECT *
        FROM {_table} t
        WHERE 0 < (
                SELECT COUNT(*)
                FROM "Members" 
                WHERE t."Id" = "Members"."ChatId" AND "Members"."UserId" = @UserId
            )
            OR 
            "Id" LIKE CONCAT('%', @UserId, '%')
        ORDER BY "LastMessageAt" DESC
        LIMIT @PageSize
        OFFSET @PageNumber * @PageSize;
    """;

    public const string NewMessage = $"""
        UPDATE {_table}
        SET "LastMessageAt" = @LastMessageAt
        WHERE "Id" = @Id;
    """;

    public const string Activate = $"""
        UPDATE {_table}
        SET "IsActive" = TRUE
        WHERE "Id" = @Id;
    """;

    public const string SoftDelete = $"""
        UPDATE {_table}
        SET "IsActive" = FALSE
        WHERE "Id" = @Id;
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "Id" = @Id;
    """;

    public const string Clear = $"DELETE FROM {_table};";
}