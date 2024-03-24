namespace ChatService.Data.Sql;

public static class MembersQueries {
    private const string _table = "Members";
    
    public const string Add = $"""
        INSERT INTO "{_table}" ("ChatId", "UserId", "Role")
        VALUES (@ChatId, @UserId, @Role)
        RETURNING "_computed";
    """;

    public const string Delete = $"""
        DELETE FROM "{_table}"
        WHERE "ChatId" = @ChatId AND "UserId" = @UserId
        RETURNING "_computed";
    """;
}