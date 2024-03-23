namespace ChatService.Data.Sql;

public static class MembersQueries {
    public const string Add = """
        INSERT INTO "Members" ("ChatId", "UserId")
        VALUES (@ChatId, @UserId);
    """;
}