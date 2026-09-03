using FluentValidation;
using sjam.RabbitMQ.Common;
using sjam.RabbitMQ.Consumer;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Services
{
    public class RabbitMQAckConsumerHostedService : BackgroundService
    {
        private readonly ILogger<RabbitMQAckConsumer> _logger;
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IValidator<AckPayloadModel> _validator;
        private readonly IConfiguration _configuration;
        private readonly string _queueName;

        private RabbitMQAckConsumer? _consumer;

        public RabbitMQAckConsumerHostedService(
            ILogger<RabbitMQAckConsumer> logger,
            IRabbitMQConnectionFactory connectionFactory,
            IServiceScopeFactory scopeFactory,
            IValidator<AckPayloadModel> validator,
            IConfiguration configuration,
            string queueName)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _scopeFactory = scopeFactory;
            _queueName = queueName;
            _validator = validator;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer = new RabbitMQAckConsumer(
                _logger,
                _connectionFactory,
                _scopeFactory,
                _validator,
                _configuration,
                _queueName);

            return _consumer.StartAsync(stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return _consumer?.StopAsync(cancellationToken) ?? Task.CompletedTask;
        }
    }
}
