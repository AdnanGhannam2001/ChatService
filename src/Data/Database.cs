using ChatService.Data.Models;
using ChatService.Data.Sql;
using Dapper;
using DbUp;
using NanoidDotNet;

namespace ChatService.Data;

public static class Database
{
    public static void Init(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);
        
        var upgrader = DeployChanges.To.PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(Database).Assembly)
            .LogToConsole()
            .Build();

        if (upgrader.IsUpgradeRequired())
        {
            upgrader.PerformUpgrade();
        }
    }

    public static async Task ClearAsync(DapperDbConnection connection)
    {
        using var db = connection.CreateConnection();
        await db.QueryAsync(MessagesQueries.Clear);
        await db.QueryAsync(MembersQueries.Clear);
        await db.QueryAsync(ChatsQueries.Clear);
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