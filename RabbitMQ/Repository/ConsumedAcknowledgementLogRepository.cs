using sjam.Dal;
using sjam.Dal.Entities;
using sjam.Dal.Repositories;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Repository
{
    public class ConsumedAcknowledgementLogRepository : Repository<ConsumeLogsAck>, IConsumedAcknowledgementLogRepository
    {
        readonly private ILogger _logger;
        readonly private EFContext _dbContext;
        public ConsumedAcknowledgementLogRepository(EFContext context, ILogger<ConsumedAcknowledgementLogRepository> logger) : base(context)
        {
            _logger = logger;
            _dbContext = context;
        }

        public async Task<bool> InsertNewLog(ConsumedLogAckModel consumedAcknowledgementLog)
        {
            try
            {
                var log = new ConsumeLogsAck()
                {
                    UniqueId = Guid.NewGuid(),
                    MessageId = consumedAcknowledgementLog.MessageId,
                    PublishedMessageId = consumedAcknowledgementLog.PublishedMessageId,
                    QueueName = consumedAcknowledgementLog.QueueName,
                    ExchangeName = consumedAcknowledgementLog.ExchangeName,
                    MessageBody = consumedAcknowledgementLog.MessageBody,
                    QueueOptions = consumedAcknowledgementLog.QueueOptions,
                    ConsumedAt = consumedAcknowledgementLog.ConsumedAt,
                    Status = consumedAcknowledgementLog.Status,
                    ErrorMessages = consumedAcknowledgementLog.ErrorMessages,
                    ErrorType = consumedAcknowledgementLog.ErrorType,
                };
                await _dbContext.AddAsync(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting consumed acknowledgement log");
                throw;
            }
        }
    }
}
