using ChatService.Data;

namespace ChatService.Extensions;

internal static class WebApplicationExtensions
{
    public static void HandleCommandArguments(this WebApplication app, string[] args)
    {
        Task.Run(async () => await HandleDatabaseArgumentsAsync(args, app))
            .Wait();
    }

    public static async Task HandleDatabaseArgumentsAsync(string[] args, WebApplication app)
    {
        var createTables = ArgumentsConstain(args, "-ct", "--create-tables");
        var seed = ArgumentsConstain(args, "-s", "--seed");

        if (!createTables && !seed) return;

        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var dbConnection = scope.ServiceProvider.GetRequiredService<DapperDbConnection>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (createTables)
        {
            logger.LogInformation("Creating Tables...");
            await Database.CreateTablesAsync(dbConnection);
            logger.LogInformation("Tables Were Created Successfully");
        }

        if (seed)
        {
            logger.LogInformation("Seeding...");
            await Database.SeedAsync(dbConnection);
            logger.LogInformation("Database Were Seeded Successfully");
        }

        Environment.Exit(0);
    }

    private static bool ArgumentsConstain(string[] args, string sflag, string lflag)
        => args.Contains(sflag) || args.Contains(lflag);
}
