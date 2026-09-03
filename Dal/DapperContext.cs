using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace sjam.Dal
{
    public class DapperContext
    {
        private readonly string _connectionString;
        private readonly string _connectionAuditLogString;
        private readonly string _connectionJitReplicationString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DBConnection")
                ?? throw new InvalidOperationException("Connection string 'DBConnection' not found.");
            _connectionAuditLogString = configuration.GetConnectionString("AuditLogDBConnection")
                ?? throw new InvalidOperationException("Connection string 'AuditLogDBConnection' not found.");
            _connectionJitReplicationString = configuration.GetConnectionString("JitReplicationDBConnection")
                ?? throw new InvalidOperationException("Connection string 'JitReplicationDBConnection' not found.");
        }

        public IDbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);
        public IDbConnection CreateAuditLogDBConnection()
           => new NpgsqlConnection(_connectionAuditLogString);
        public IDbConnection CreateJitReplicationDBConnection()
           => new NpgsqlConnection(_connectionJitReplicationString);
    }
}
