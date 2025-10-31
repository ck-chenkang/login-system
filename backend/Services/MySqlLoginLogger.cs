using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace backend.Services
{
    public class MySqlLoginLogger : ILoginLogger
    {
        private readonly string _connString;

        public MySqlLoginLogger(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("Default")
                           ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured");
        }

        public async Task EnsureTableAsync()
        {
            const string sql = @"CREATE TABLE IF NOT EXISTS login_logs (
                id BIGINT PRIMARY KEY AUTO_INCREMENT,
                username VARCHAR(255) NOT NULL,
                success TINYINT(1) NOT NULL,
                ip VARCHAR(64) NULL,
                user_agent VARCHAR(512) NULL,
                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task LogAsync(string username, bool success, string? ip, string? userAgent)
        {
            const string insert = @"INSERT INTO login_logs (username, success, ip, user_agent) VALUES (@u, @s, @ip, @ua);";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(insert, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@s", success);
            cmd.Parameters.AddWithValue("@ip", (object?)ip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ua", (object?)userAgent ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

