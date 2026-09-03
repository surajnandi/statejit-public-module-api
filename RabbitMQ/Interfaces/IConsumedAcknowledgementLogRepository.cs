using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Interfaces
{
    public interface IConsumedAcknowledgementLogRepository : IRepository<ConsumeLogsAck>
    {
        Task<bool> InsertNewLog(ConsumedLogAckModel consumedAcknowledgementLog);
    }
}
