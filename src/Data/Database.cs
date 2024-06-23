using ChatService.Data.Sql;
using ChatService.Models;
using Dapper;
using NanoidDotNet;

namespace ChatService.Data;

public static class Database
{
    public async static Task CreateTablesAsync(DapperDbConnection connection)
    {
        using var db = connection.CreateConnection();

        await db.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "Chats" (
                "Id" VARCHAR(255) NOT NULL,
                "IsGroup" BOOLEAN DEFAULT FALSE,
                "IsActive" BOOLEAN DEFAULT TRUE,
                "LastMessageAt" DATE,
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
    }

    public static async Task SeedAsync(DapperDbConnection connection)
    {
        using var db = connection.CreateConnection();
        var random = new Random();

        var users = new List<string>();

        for (var i = 0; i < 10; ++i)
        {
            users.Add(Nanoid.Generate(size: 15));
        }

        for (var i = 0; i < 3; ++i)
        {
            var chat = new Chat(Nanoid.Generate(size: 15), members: []);

            await db.QueryAsync(ChatsQueries.Add, chat);

            var membersIds = users.OrderBy(u => random.Next()).Take(3);
            foreach (var memberId in membersIds)
            {
                var member = new Member(chat.Id, memberId);

                await db.QueryAsync(MembersQueries.Add, member);
            }
        }
    }
}