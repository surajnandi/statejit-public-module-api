using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using sjam.RabbitMQ.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace sjam.RabbitMQ.Common
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _factory;

        public RabbitMqService(IConfiguration configuration)
        {
            _configuration = configuration;
            _factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQConnection:Host"],
                Port = int.Parse(_configuration["RabbitMQConnection:Port"]),
                UserName = _configuration["RabbitMQConnection:UserName"],
                Password = _configuration["RabbitMQConnection:Password"],
                VirtualHost = _configuration["RabbitMQConnection:VirtualHost"]
            };
        }

        public async Task PublishAsync<T>(string routingKey, string UniqueId, T message, string exchange = "") where T : class
        {
            using (var connection = await _factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                await channel.QueueDeclareAsync(
                    queue: routingKey,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var props = new BasicProperties
                {
                    MessageId = UniqueId,
                    //CorrelationId = messageId,
                    ReplyTo = $"{routingKey}_ack",
                    DeliveryMode = DeliveryModes.Persistent
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                await channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: body);
            }
        }

        public async Task<string> GetPayloadRabbitMQAsync<T>(string queueName)
        {
            using (var connection = await _factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Declare the queue (ensure it exists)
                await channel.QueueDeclareAsync(queue: queueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                // Use BasicGet to get a message from the queue (non-blocking)
                var result = await channel.BasicGetAsync(queue: queueName, autoAck: true);

                if (result == null)
                {
                    return null; // No message in the queue
                }

                var body = result.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                return message; // Return the message
            }
        }


        public async Task<string> PushMessageAsync(string queueName, string queueId, string message, string exchange = "")
        {

            using (IConnection connection = await _factory.CreateConnectionAsync())
            {
                using IChannel channel = await connection.CreateChannelAsync();
                var properties = new BasicProperties();
                properties.MessageId = queueId;
                await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange: exchange, routingKey: queueName, mandatory: true, basicProperties: properties, body: messageBytes);
                //Console.WriteLine(" [x] Sent '" + message + "'");
            }

            return "Message published to the queue successfully.";
        }

        public async Task<string> PushLegacyMessageAsync(string queueName, string queueId, string message, string exchange = "")
        {
            if (string.IsNullOrWhiteSpace(message))
                return "";

            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var properties = new BasicProperties
            {
                MessageId = queueId
            };

            try
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                string? correlationId = null;

                // Array -> take first element
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var first = root[0];
                    if (first.TryGetProperty("CorrelationId", out var cid))
                        correlationId = cid.GetString();
                }
                //  Object
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("CorrelationId", out var cid))
                        correlationId = cid.GetString();

                }

                if (!string.IsNullOrEmpty(correlationId))
                {
                    properties.CorrelationId = correlationId;
                }
            }
            catch (Exception ex)
            {
                // ?? Do NOT fail publishing � just log
                Console.WriteLine($"[Warning] Failed to extract CorrelationId: {ex.Message}");
            }

            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

            var messageBytes = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: messageBytes
            );

            return "Message Published";
        }

    }
}
