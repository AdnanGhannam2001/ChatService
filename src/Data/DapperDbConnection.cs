using Npgsql;

namespace ChatService.Data;

public sealed class DapperDbConnection
{
    private static readonly string name = "PostgresConnection";
    private readonly string? _connectionString;

    public DapperDbConnection(IConfiguration configuration) => _connectionString = configuration.GetConnectionString(name);

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}