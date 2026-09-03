using AutoMapper;
using sjam.Dal;
using sjam.Dal.Entities;
using sjam.Dal.Repositories;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Repository
{
    public class ConsumeFailedLogRepository : Repository<ConsumeFailedLog>, IConsumeFailedLogRepository
    {
        private readonly EFContext _dbContext;
        private readonly IMapper _mapper;

        public ConsumeFailedLogRepository(EFContext context, IMapper mapper) : base(context)
        {
            _dbContext = context;
            _mapper = mapper;
        }
        public async Task InsertNewLog(ConsumedLogModel newConsumeLog)
        {
            ConsumeFailedLog consumeLog = _mapper.Map<ConsumeFailedLog>(newConsumeLog);
            //ConsumeFailedLog consumeFailedLog = new ConsumeFailedLog
            //{
            //    MessageId = Guid.Parse(newConsumeLog.MessageId),
            //    QueueName = newConsumeLog.QueueName,
            //    ActionStatus = newConsumeLog.ActionStatus,
            //    ConsumedAt = newConsumeLog.ConsumedAt,
            //    FailedAt = newConsumeLog.FailedAt,
            //    ExchangeName = newConsumeLog.ExchangeName,
            //    FailedMessage = newConsumeLog.FailedMessage,
            //    FailedType = newConsumeLog.FailedType,
            //    RaoutingKey = newConsumeLog.RaoutingKey,
            //    MessageBody = newConsumeLog.MessageBody,
            //};
            await _dbContext.ConsumeFailedLogs.AddAsync(consumeLog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
