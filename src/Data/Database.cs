using Dapper;

namespace ChatService.Data;

public static class Database {
    public async static Task CreateTablesAsync(DapperDbConnection connection) {
        using var db = connection.CreateConnection();

        await db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "Chats" (
                "Id" VARCHAR(255) NOT NULL,
                "IsGroup" BOOLEAN DEFAULT FALSE,
                PRIMARY KEY ("Id")
            );
        """);

        await db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "Members" (
                "ChatId" VARCHAR(255) NOT NULL,
                "UserId" VARCHAR(255) NOT NULL,
                "Role" INTEGER NOT NULL,
                PRIMARY KEY ("ChatId", "UserId"),
                FOREIGN KEY ("ChatId") REFERENCES "Chats"("Id")
            );
        """);

        await db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "Messages" (
                "Id" VARCHAR(255) NOT NULL,
                "SenderId" VARCHAR(255) NOT NULL,
                "ChatId" VARCHAR(255) NOT NULL,
                "Content" VARCHAR(1000) NOT NULL, 
                "SentAt" DATE,
                "LastUpdateAt" DATE,
                PRIMARY KEY ("Id"),
                FOREIGN KEY ("ChatId") REFERENCES "Chats"("Id")
            );
        """);

        await db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "Friendships" (
                "UserId" VARCHAR(255) NOT NULL,
                "FriendId" VARCHAR(255) NOT NULL,
                PRIMARY KEY ("UserId", "FriendId")
            );
        """);
    }

    public async static Task SeedAsync(DapperDbConnection connection) { }
}