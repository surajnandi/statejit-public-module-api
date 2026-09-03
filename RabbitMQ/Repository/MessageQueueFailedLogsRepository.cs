using AutoMapper;
using sjam.Dal;
using sjam.Dal.Entities;
using sjam.Dal.Repositories;
using sjam.RabbitMQ.Enums;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Repository
{
    public class MessageQueueFailedLogsRepository : Repository<PublishFailedLog>, IMessageQueueFailedLogsRepository
    {
        private readonly EFContext _dbContext;
        private readonly IMapper _mapper;

        public MessageQueueFailedLogsRepository(EFContext context, IMapper mapper) : base(context)
        {
            _dbContext = context;
            _mapper = mapper;
        }

        public async Task InsertLogAsync(AckPayloadModel ackPayload)
        {
            //MessageQueueFailedLog queueFailedLog = _mapper.Map<MessageQueueFailedLog>(ackPayload);
            var queueFailedLog = new PublishFailedLog
            {
                UniqueId = Guid.NewGuid(),
                MessageId = ackPayload.MessageId,
                ActionStatus = ConsumeStatusEnums.PENDING,
                FailedType = ackPayload.FailedType,
                FailedAt = ackPayload.Timestamp.ToUniversalTime(),
                FailedMessage = ackPayload.StatusMsg
            };

            await _dbContext.PublishFailedLogs.AddAsync(queueFailedLog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
