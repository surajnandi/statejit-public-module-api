using AutoMapper;
using sjam.Dal;
using sjam.Dal.Entities;
using sjam.Dal.Repositories;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Repository
{
    public class ConsumeLogRepository : Repository<ConsumeLog>, IConsumeLogRepository
    {
        private readonly EFContext _dbContext;
        private readonly IMapper _mapper;

        public ConsumeLogRepository(EFContext context, IMapper mapper) : base(context)
        {
            _dbContext = context;
            _mapper = mapper;
        }
        public async Task InsertNewLog(ConsumedLogModel newConsumeLog)
        {
            ConsumeLog consumeLog = _mapper.Map<ConsumeLog>(newConsumeLog);
            await _dbContext.ConsumeLogs.AddAsync(consumeLog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
