namespace sjam.RabbitMQ.Common
{
    public interface IRabbitMqService
    {
        //void Publish<T>(string routingKey, T message, string exchange = "") where T : class;
        //void Consume<T>(string queueName, Action<T> handleMessage) where T : class;
        Task PublishAsync<T>(string routingKey, string UniqueId, T message, string exchange = "") where T : class;
        Task<string> GetPayloadRabbitMQAsync<T>(string queueName);
        Task<string> PushMessageAsync(string queueName, string queueId, string message, string exchange = "");
        Task<string> PushLegacyMessageAsync(string queueName, string queueId, string message, string exchange = "");
    }
}
