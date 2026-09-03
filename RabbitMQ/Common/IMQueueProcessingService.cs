namespace sjam.RabbitMQ.Common
{
    public interface IMQueueProcessingService
    {
        Task ProcessQueueAsync(string queueName);
        Task ProcessLegacyQueueAsync(string queueName);
    }
}
