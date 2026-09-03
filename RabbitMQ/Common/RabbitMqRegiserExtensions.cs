using FluentValidation;
using Microsoft.Extensions.Options;
using sjam.RabbitMQ.Consumer;
using sjam.RabbitMQ.Enums;
using sjam.RabbitMQ.Models;
using sjam.RabbitMQ.Models.RabbitMQModel;
using sjam.RabbitMQ.Services;
using sjam.RabbitMQ.Validators;

namespace sjam.RabbitMQ.Common
{
    public static class RabbitMqRegiserExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMQConfig = configuration.GetSection("RabbitMQConnection").Get<RabbitMQConfigurationModel>();
            if (rabbitMQConfig == null)
            {
                throw new InvalidOperationException("RabbitMQ configuration is missing");
            }

            services.AddSingleton(rabbitMQConfig);

            if (!rabbitMQConfig.Enabled)
            {
                Console.WriteLine("RabbitMQ service is disabled. RabbitMQ consumers will not start.");

                return services;
            }

            Console.WriteLine("RabbitMQ service is enabled. Starting RabbitMQ consumers...");

            services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();

            return services;
        }

        public static IServiceCollection AddMessageProcessing(this IServiceCollection services, IConfiguration configuration)
        {
            //var rabbitMQConfig = configuration.GetSection("RabbitMQConnection").Get<RabbitMQConfigurationModel>();
            //bool enabled = rabbitMQConfig?.Enabled ?? true;

            var rabbitMQConfig = configuration.GetSection("RabbitMQConnection").Get<RabbitMQConfigurationModel>();

            if (rabbitMQConfig == null)
            {
                throw new InvalidOperationException("RabbitMQ configuration is missing");
            }

            if (!rabbitMQConfig.Enabled)
            {
                Console.WriteLine("RabbitMQ service is disabled. RabbitMQ consumers will not start.");

                return services;
            }

            #region CONSUMER

            services.AddScoped<IValidator<TestQueueConsumeDto>, TestQueueConsumeDtoValidator>();
            services.AddScoped<IMessageProcessor<TestQueueConsumeDto>, TestQueueConsumeService>();
            services.AddHostedService<TestQueueConsumer>();


            #endregion CONSUMER


            #region Generic ACK Consumer

            services.AddSingleton<IValidator<AckPayloadModel>, MQueueAckValidator>();
            //var MessageQueues = new[]
            //{
            //    RabbitMqQueueName.TEST_QUEUE_PUBLISH_ACK
            //    //RabbitMqQueueName.TEST_QUEUE_CONSUME_ACK
            //};

            var messageQueues = Array.Empty<string>();

            // If ACK consumer is required:
            // var messageQueues = new[]
            // {
            //     RabbitMqQueueName.TEST_QUEUE_PUBLISH_ACK
            // };

            //var queues = config.GetSection("MessageQueues").Get<List<string>>() ?? new();
            foreach (var queue in messageQueues)
            {

                services.AddSingleton<IHostedService>(sp =>
                     new RabbitMQAckConsumerHostedService(
                         sp.GetRequiredService<ILogger<RabbitMQAckConsumer>>(),
                         sp.GetRequiredService<IRabbitMQConnectionFactory>(),
                         sp.GetRequiredService<IServiceScopeFactory>(),
                         sp.GetRequiredService<IValidator<AckPayloadModel>>(),
                         sp.GetRequiredService<IConfiguration>(),
                         queue
                     ));
            }

            #endregion

            return services;
        }
    }
}
