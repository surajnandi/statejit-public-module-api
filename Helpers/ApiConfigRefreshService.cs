using Npgsql;
using sjam.Dal;

namespace sjam.Helpers
{
    public sealed class ApiConfigRefreshService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiConfigRefreshService> _logger;

        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        public ApiConfigRefreshService(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<ApiConfigRefreshService> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RefreshCacheAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var conn =
                        new NpgsqlConnection(
                            _configuration.GetConnectionString("DBConnection"));

                    await conn.OpenAsync(stoppingToken);

                    conn.Notification += async (_, e) =>
                    {
                        if (e.Channel == "config_master_change")
                        {
                            await RefreshCacheAsync();
                        }
                    };

                    await using var cmd =
                        new NpgsqlCommand(
                            "LISTEN config_master_change;",
                            conn);

                    await cmd.ExecuteNonQueryAsync(stoppingToken);

                    _logger.LogInformation(
                        "Api Control Listener Started");

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await conn.WaitAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Listener disconnected. Reconnecting in 5 seconds.");

                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
            }
        }

        private async Task RefreshCacheAsync()
        {
            if (!await _refreshLock.WaitAsync(0))
                return;

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var dapperContext =
                    scope.ServiceProvider
                         .GetRequiredService<DapperContext>();

                await ApiConfigHelper.LoadAsync(dapperContext);

                _logger.LogInformation(
                    "Api Control Cache Refreshed");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while refreshing API control cache");
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}
