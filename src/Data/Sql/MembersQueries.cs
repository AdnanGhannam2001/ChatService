namespace ChatService.Data.Sql;

public static class MembersQueries
{
    private const string _table = "\"Members\"";

    public const string Get = $"""
        SELECT *
        FROM {_table}
        WHERE "ChatId" = @ChatId AND "UserId" = @UserId;
    """;

    public const string Add = $"""
        INSERT INTO {_table} ("ChatId", "UserId", "Role")
        VALUES (@ChatId, @UserId, @Role);
    """;

    public const string ChangeRole = $"""
        UPDATE {_table}
        SET "Role" = @Role
        WHERE "ChatId" = @ChatId AND "UserId" = @UserId;
    """;

    public const string Delete = $"""
        DELETE FROM {_table}
        WHERE "ChatId" = @ChatId AND "UserId" = @UserId;
    """;
}