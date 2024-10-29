using ChatService.Constants;
using Npgsql;

namespace ChatService.Data;

public sealed class DapperDbConnection
{
    private readonly string? _connectionString;

    public DapperDbConnection(IConfiguration configuration) =>
        _connectionString = configuration.GetConnectionString(CommonConstants.ConnectionStringName);

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}