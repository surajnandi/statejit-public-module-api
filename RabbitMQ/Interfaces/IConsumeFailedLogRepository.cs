using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Interfaces
{
    public interface IConsumeFailedLogRepository : IRepository<ConsumeFailedLog>
    {
        Task InsertNewLog(ConsumedLogModel newConsumeLog);
    }
}
