using sjam.RabbitMQ.Common;
using sjam.RabbitMQ.Enums;
using sjam.RabbitMQ.Models;

namespace sjam.RabbitMQ.Consumer
{
    public class TestQueueConsumer : RabbitMQConsumerBase<TestQueueConsumeDto>
    {
        public TestQueueConsumer
        (
            ILogger<TestQueueConsumer> logger,
            IRabbitMQConnectionFactory connectionFactory,
            IServiceScopeFactory serviceScopeFactory
        ) 
        : base
        (
           logger,
           connectionFactory,
           serviceScopeFactory,
            // configuration, 
            // errorLogger, 
            RabbitMqQueueName.TEST_QUEUE_CONSUME
        )
        { }
    }
}
