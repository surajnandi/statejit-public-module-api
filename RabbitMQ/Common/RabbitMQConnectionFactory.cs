using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using sjam.RabbitMQ.Models;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Common
{
    public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory, IDisposable
    {
        private readonly RabbitMQConfigurationModel _configuration;
        private readonly ILogger<RabbitMQConnectionFactory> _logger;
        private IConnection? _connection;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);

        public RabbitMQConnectionFactory(
            RabbitMQConfigurationModel configuration,
            ILogger<RabbitMQConnectionFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection?.IsOpen == true)
                return _connection;

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_connection?.IsOpen == true)
                    return _connection;

                var factory = new ConnectionFactory
                {
                    HostName = _configuration.Host,
                    UserName = _configuration.UserName,
                    Password = _configuration.Password,
                    VirtualHost = _configuration.VirtualHost,
                    Port = _configuration.Port,
                    ConsumerDispatchConcurrency = 1
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _logger.LogInformation("Successfully connected to RabbitMQ");

                _connection.ConnectionShutdownAsync += OnConnectionShutdown;

                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
        {
            var connection = await CreateConnectionAsync(cancellationToken);
            return await connection.CreateChannelAsync();
        }

        private Task OnConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection shutdown. Reason: {0}", e.ReplyText);
            _connection = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connectionLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
