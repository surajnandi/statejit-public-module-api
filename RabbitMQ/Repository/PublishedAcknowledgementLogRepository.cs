using sjam.Dal;
using sjam.Dal.Entities;
using sjam.Dal.Repositories;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Repository
{
    public class PublishedAcknowledgementLogRepository : Repository<PublishLogsAck>, IPublishedAcknowledgementLogRepository
    {
        private readonly EFContext _dbContext;
        private readonly ILogger _logger;
        public PublishedAcknowledgementLogRepository(EFContext context, ILogger<PublishedAcknowledgementLogRepository> logger) : base(context)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<bool> InsertNewLog(PublishedLogAckModel publishedAcknowledgementLog)
        {
            try
            {
                var log = new PublishLogsAck()
                {
                    UniqueId = publishedAcknowledgementLog.UniqueId,
                    ConsumeMessageId = publishedAcknowledgementLog.MessageId,
                    QueueName = publishedAcknowledgementLog.QueueName,
                    ExchangeName = publishedAcknowledgementLog.ExchangeName,
                    MessageBody = publishedAcknowledgementLog.MessageBody,
                    QueueOptions = publishedAcknowledgementLog.QueueOptions,
                    PublishAt = publishedAcknowledgementLog.PublishAt,
                };
                await _dbContext.AddAsync(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting published acknowledgement log");
                throw;
            }
        }
    }
}
