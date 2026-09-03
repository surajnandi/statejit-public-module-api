using sjam.Dal.Entities;
using sjam.Dal.Interfaces;

namespace sjam.RabbitMQ.Interfaces
{
    public interface IMessageQueueRepository : IRepository<PendingLog>
    {
        Task<IEnumerable<PendingLog>> GetRecordsForQueueAsync(string queueName);
        Task InsertLogAsync(PendingLog record, string queueName);
        Task RemoveRecordAsync(Guid uniqueId);
    }
}
