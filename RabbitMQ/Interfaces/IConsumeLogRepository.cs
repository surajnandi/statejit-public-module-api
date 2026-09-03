using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Interfaces
{
    public interface IConsumeLogRepository : IRepository<ConsumeLog>
    {
        Task InsertNewLog(ConsumedLogModel newConsumeLog);
    }
}
