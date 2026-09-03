using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using sjam.Helpers;
using sjam.RabbitMQ.Enums;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;
using System.Text;
using System.Text.Json;

namespace sjam.RabbitMQ.Common
{
    public abstract class RabbitMQConsumerBase<T> : BackgroundService where T : class
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ILogger _logger;
        private readonly string _queueName;
        private readonly string _defaultAckQueueName;
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string? _exchangeName;

        private sealed class MessageHandlerContext
        {
            public required ConsumedLogModel ConsumeLog { get; init; }
            public required IReadOnlyBasicProperties BasicProperties { get; init; }
            public required string AckQueueName { get; set; }
            public required IMessageProcessor<T> MessageProcessor { get; init; }
            public required IConsumeLogRepository ConsumeLogRepo { get; init; }
            public required IConsumeFailedLogRepository ConsumeFailedLogRepo { get; init; }
            public required IPublishedAcknowledgementLogRepository PublishedAcknowledgementLogRepo { get; init; }
        }

        protected RabbitMQConsumerBase(
            ILogger logger,
            IRabbitMQConnectionFactory connectionFactory,
            IServiceScopeFactory serviceScopeFactory,
            string queueName,
            string? exchangeName = null,
            string? routingKey = null,
            bool isDurable = true)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _queueName = queueName;
            _exchangeName = exchangeName;
            _defaultAckQueueName = $"{_queueName}_ack";
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndConsume(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in consumer execution. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ConnectAndConsume(CancellationToken stoppingToken)
        {
            _connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
            _channel = await _connectionFactory.CreateChannelAsync(stoppingToken);

            // Configure channel
            await _channel.QueueDeclareAsync(_queueName,
                 durable: true,
                 exclusive: false,
                 autoDelete: false);
            if (!string.IsNullOrWhiteSpace(_exchangeName))
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Fanout, // You can change to Topic/Fanout if required
                    durable: true,
                    autoDelete: false);
                await _channel.QueueBindAsync(
                   queue: _queueName,
                   exchange: _exchangeName,
                     routingKey: "");
            }

            await _channel.BasicQosAsync(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += HandleMessageAsync;

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);

            // Keep the connection alive until cancellation is requested
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                _logger.LogInformation("Consumer shutdown initiated");
            }
        }

        private async Task HandleMessageAsync(object? sender, BasicDeliverEventArgs ea)
        {
            // Capture raw bytes immediately — before any code that could throw
            var rawBody = ea.Body.ToArray();
            MessageHandlerContext? ctx = null;

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                ctx = new MessageHandlerContext
                {
                    ConsumeLog = new ConsumedLogModel { QueueName = _queueName, ConsumedAt = DateTime.Now },
                    BasicProperties = ea.BasicProperties,
                    AckQueueName = ea.BasicProperties.ReplyTo ?? _defaultAckQueueName,
                    MessageProcessor = scope.ServiceProvider.GetRequiredService<IMessageProcessor<T>>(),
                    ConsumeLogRepo = scope.ServiceProvider.GetRequiredService<IConsumeLogRepository>(),
                    ConsumeFailedLogRepo = scope.ServiceProvider.GetRequiredService<IConsumeFailedLogRepository>(),
                    PublishedAcknowledgementLogRepo = scope.ServiceProvider.GetRequiredService<IPublishedAcknowledgementLogRepository>(),
                };

                var message = Encoding.UTF8.GetString(rawBody);
                ctx.ConsumeLog.IsRedelivered = ea.Redelivered;
                ctx.ConsumeLog.MessageBody = message;

                _logger.LogInformation("Processing message. Redelivered: {IsRedelivered}", ea.Redelivered);
                if (await ProcessMessageAsync(ctx))
                {
                    await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                }
                else
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag, false, !ea.Redelivered);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing message from queue {QueueName}", _queueName);

                if (ctx != null)
                {
                    // Context exists — try normal failed-process path (DB → local fallback)
                    await FailedProcess(ctx, "ProcessingError", ex.ToString(), false, ConsumeStatusEnums.PENDING);
                }
                else
                {
                    // Context not built (e.g. DI failure) — save raw bytes directly to disk
                    SaveRawMessageLocally("DISetupError", rawBody, ea, ex);
                }

                if (_channel != null)
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        }

        private void SaveRawMessageLocally(string reason, byte[] rawBody, BasicDeliverEventArgs ea, Exception ex)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                string? basePath = configuration["Logging:MessageQueueLog:ErrorLogPath"];
                if (!string.IsNullOrEmpty(basePath) && !basePath.EndsWith('/') && !basePath.EndsWith('\\'))
                    basePath += "/";

                var data = JsonSerializer.Serialize(new
                {
                    _queueName,
                    ea.DeliveryTag,
                    ea.BasicProperties.MessageId,
                    ea.Redelivered,
                    RawBody = Convert.ToBase64String(rawBody),
                    Body = Encoding.UTF8.GetString(rawBody),
                    CapturedAt = DateTime.Now,
                });

                string path = $"{basePath}RawMessages/{_queueName}_{reason}_{ea.DeliveryTag}_{DateTime.Now:yyyyMMddHHmmss}";
                MqErrorFileLogger.SaveErrorLocally(path, data, ex.ToString());
            }
            catch (Exception saveEx)
            {
                _logger.LogCritical(saveEx, "CRITICAL: Failed to save raw message to disk. Message from queue {QueueName} may be lost. DeliveryTag={DeliveryTag}", _queueName, ea.DeliveryTag);
            }
        }

        private async Task<bool> ProcessMessageAsync(MessageHandlerContext ctx)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ctx.BasicProperties.MessageId))
                {
                    await FailedProcess(ctx, "MessageIdMissing", "MessageId is missing in the message properties");
                    return true;
                }

                if (Guid.TryParse(ctx.BasicProperties.MessageId, out var msgId))
                {
                    ctx.ConsumeLog.MessageId = msgId;
                }
                else
                {
                    await FailedProcess(ctx, "InvalidMessageId", "MessageId is not a valid GUID");
                    return true;
                }
                var payload = JsonSerializer.Deserialize<T>(ctx.ConsumeLog.MessageBody, _jsonOptions);

                if (payload == null)
                {
                    await FailedProcess(ctx, "Deserialization", "Message deserialization resulted in null");
                    return true;
                }

                var validationResult = await ctx.MessageProcessor.ValidateMessage(payload);
                if (!validationResult.IsValid)
                {
                    var validationErrors = validationResult.Errors
                        .Select(error => new ValidationError
                        {
                            PropertyName = error.PropertyName,
                            ErrorMessage = error.ErrorMessage
                        });
                    await FailedProcess(ctx, "ValidationFailure", JsonSerializer.Serialize(validationErrors));
                    return true;
                }
                await ctx.MessageProcessor.ProcessMessage(payload, ctx.BasicProperties);
                await SuccessProcess(ctx);
                return true;
            }
            catch (JsonException ex)
            {
                await FailedProcess(ctx, "Deserialization", ex.Message);
                return true;
            }
            catch (Exception ex)
            {
                await FailedProcess(ctx, "ProcessingError", ex.ToString(), false, ConsumeStatusEnums.PENDING);
                return false;
            }
        }

        private async Task SuccessProcess(MessageHandlerContext ctx, bool isPublish = true)
        {
            try
            {
                ctx.ConsumeLog.Status = ConsumeStatusEnums.SUCCESS;
                ctx.ConsumeLog.ExchangeName = _exchangeName;
                await ctx.ConsumeLogRepo.InsertNewLog(ctx.ConsumeLog);

                if (isPublish)
                {
                    await PublishAckAsync(ctx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert consume success log for MessageId={MessageId}", ctx.ConsumeLog.MessageId);
                SaveLocally("ConsumeLog", ctx.ConsumeLog.MessageId, JsonSerializer.Serialize(ctx.ConsumeLog), ex.ToString());
            }
        }

        private async Task FailedProcess(
            MessageHandlerContext ctx,
            string failedType,
            string failedMessage,
            bool isPublish = true,
            string actionStatus = ConsumeStatusEnums.NO_ACTION)
        {
            try
            {
                ctx.ConsumeLog.FailedType = failedType;
                ctx.ConsumeLog.FailedMessage = failedMessage;
                ctx.ConsumeLog.FailedAt = DateTime.Now;
                ctx.ConsumeLog.ActionStatus = actionStatus;
                ctx.ConsumeLog.ExchangeName = _exchangeName;
                await ctx.ConsumeFailedLogRepo.InsertNewLog(ctx.ConsumeLog);
                if (isPublish)
                {
                    await PublishNackAsync(ctx, failedType, failedMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert consume failed log for MessageId={MessageId}", ctx.ConsumeLog.MessageId);
                SaveLocally("consumeFailedLog", ctx.ConsumeLog.MessageId, JsonSerializer.Serialize(ctx.ConsumeLog), ex.ToString());
            }
        }

        private async Task PublishAcknowledgementAsync(
            MessageHandlerContext ctx,
            Guid? consumeMessageQueueId,
            string ackMessageId,
            string routingKey,
            bool isSuccess,
            string statusMessage = "",
            string failedType = "")
        {
            AckPayloadNullGuidModel ackPayload = new AckPayloadNullGuidModel();
            BasicProperties basicProperties = new BasicProperties();
            try
            {
                DateTime timestamp = DateTime.Now;
                ackPayload = new AckPayloadNullGuidModel
                {
                    MessageId = consumeMessageQueueId,
                    Status = isSuccess ? ConsumeStatusEnums.SUCCESS : ConsumeStatusEnums.FAILED,
                    StatusMsg = isSuccess ? "Message Consumed Successfully." : statusMessage,
                    FailedType = isSuccess ? null : failedType,
                    Timestamp = timestamp,
                };

                var ackPayloadString = JsonSerializer.Serialize(ackPayload);
                var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(ackPayloadString));

                basicProperties = new BasicProperties
                {
                    MessageId = ackMessageId,
                    AppId = ConsumeStatusEnums.ModuleId, // sends "50" 
                    CorrelationId = ctx.BasicProperties.CorrelationId,
                };

                if (_channel == null)
                {
                    throw new Exception("Channel is null");
                }

                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: routingKey,
                    mandatory: true,  // true for NACK, false for ACK
                    basicProperties: basicProperties,
                    body: body
                );

                await InsertAckLogAsync(ctx, ackPayload, basicProperties);
            }
            catch (Exception ex)
            {
                var data = new
                {
                    AckPayload = ackPayload,
                    BasicProperties = basicProperties
                };
                SaveLocally("PublishAcknowledgementAsync", consumeMessageQueueId, JsonSerializer.Serialize(data), ex.ToString());
            }
        }

        private async Task PublishNackAsync(MessageHandlerContext ctx, string failedType, string failedMessage)
        {
            await PublishAcknowledgementAsync(
                ctx,
                consumeMessageQueueId: ctx.ConsumeLog.MessageId,
                ackMessageId: Guid.NewGuid().ToString(),
                routingKey: ctx.AckQueueName,
                isSuccess: false,
                statusMessage: failedMessage,
                failedType: failedType
            );
        }

        private async Task PublishAckAsync(MessageHandlerContext ctx)
        {
            await PublishAcknowledgementAsync(
                ctx,
                consumeMessageQueueId: ctx.ConsumeLog.MessageId,
                ackMessageId: Guid.NewGuid().ToString(),
                routingKey: ctx.AckQueueName,
                isSuccess: true
            );
        }

        private void SaveLocally(string folderName, Guid? queueId, string data, string error)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                string? basePath = configuration["Logging:MessageQueueLog:ErrorLogPath"];

                if (!string.IsNullOrEmpty(basePath) && !basePath.EndsWith('/') && !basePath.EndsWith('\\'))
                    basePath += "/";

                string path = $"{basePath}{folderName}/{_queueName}_{queueId}";
                MqErrorFileLogger.SaveErrorLocally(path, data, error);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "CRITICAL: SaveLocally failed for queue {QueueName}, folder {FolderName}, id {QueueId}. Message data may be lost.", _queueName, folderName, queueId);
            }
        }

        private async Task InsertAckLogAsync(MessageHandlerContext ctx, AckPayloadNullGuidModel ackPayload, BasicProperties basicProperties)
        {
            try
            {
                PublishedLogAckModel publishedAcknowledgementLogModel = new PublishedLogAckModel()
                {
                    UniqueId = Guid.Parse(basicProperties.MessageId),
                    MessageId = ackPayload.MessageId ?? Guid.Empty,
                    QueueName = ctx.AckQueueName,
                    MessageBody = JsonSerializer.Serialize(ackPayload),
                    QueueOptions = JsonSerializer.Serialize(basicProperties),
                    PublishAt = ackPayload.Timestamp
                };
                var result = await ctx.PublishedAcknowledgementLogRepo.InsertNewLog(publishedAcknowledgementLogModel);
                if (!result)
                {
                    throw new Exception("Failed to insert published acknowledgement log table");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert published acknowledgement log table", ex);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping consumer");
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
