using RabbitMQ.Client;

namespace sjam.RabbitMQ.Common
{
    public interface IRabbitMQConnectionFactory
    {
        Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
        Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
    }
}
