using sjam.Dal.Entities;
using sjam.Dal.Interfaces;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.RabbitMQ.Interfaces
{
    public interface IPublishedAcknowledgementLogRepository : IRepository<PublishLogsAck>
    {
        Task<bool> InsertNewLog(PublishedLogAckModel publishedAcknowledgementLog);
    }
}
