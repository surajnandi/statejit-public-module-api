using Dapper;
using sjam.Dal;
using sjam.Models;
using System.Collections.Concurrent;

namespace sjam.Helpers
{
    public static class ApiConfigHelper
    {
        public static readonly ConcurrentDictionary<string, ApiConfigMaster>
            Cache = new();

        public static async Task LoadAsync(DapperContext dapperContext)
        {
            using var connection = dapperContext.CreateConnection();

            const string sql = @"
                SELECT
                    controller_name AS ControllerName,
                    action_name AS ActionName,
                    is_active AS IsActive,
                    message AS Message,
                    scheduled_start AS ScheduledStart,
                    scheduled_end AS ScheduledEnd,
                    fin_year AS FinYear
                FROM master.config_master;";

            var data = await connection.QueryAsync<ApiConfigMaster>(sql);

            Cache.Clear();

            foreach (var item in data)
            {
                Cache.TryAdd(
                    $"{item.ControllerName}:{item.ActionName ?? "*"}"
                    .ToUpperInvariant(),
                    item);
            }
        }

        public static bool TryGet(string controller, string action, out ApiConfigMaster config)
        {
            return
                Cache.TryGetValue(
                    $"{controller}:{action}".ToUpperInvariant(),
                    out config)
                ||
                Cache.TryGetValue(
                    $"{controller}:*".ToUpperInvariant(),
                    out config)
                ||
                Cache.TryGetValue(
                    "*:*",
                    out config);
        }
    }
}
