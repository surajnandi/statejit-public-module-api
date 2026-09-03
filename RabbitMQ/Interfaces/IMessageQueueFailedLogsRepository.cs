using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Interfaces
{
    public interface IMessageQueueFailedLogsRepository : IRepository<PublishFailedLog>
    {
        Task InsertLogAsync(AckPayloadModel record);
    }
}
