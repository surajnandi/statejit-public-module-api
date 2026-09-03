using Microsoft.EntityFrameworkCore;
using sjam.Dal;
using sjam.Dal.Entities;
using sjam.Dal.Repositories;
using sjam.RabbitMQ.Interfaces;

namespace sjam.RabbitMQ.Repository
{
    public class MessageQueueRepository : Repository<PendingLog>, IMessageQueueRepository
    {
        private readonly EFContext _dbContext;

        public MessageQueueRepository(EFContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task<IEnumerable<PendingLog>> GetRecordsForQueueAsync(string queueName)
        {
            return await _dbContext.PendingLogs
                .Where(r => r.QueueName == queueName)
                .ToListAsync();
        }

        public async Task InsertLogAsync(PendingLog record, string queueName)
        {
            try
            {
                var sql = @"
                    INSERT INTO rabbitmq.publish_logs 
                        (unique_id, exchange_name, message_body, publish_at, queue_name, queue_options)
                    VALUES 
                        (@p0, @p1, CAST(@p2 AS jsonb), @p3, @p4, CAST(@p5 AS jsonb))
                    RETURNING created_at;
                ";
                await _dbContext.Database.ExecuteSqlRawAsync(sql,
                    record.UniqueId,
                    record.ExchangeName ?? "",
                    record.MessageBody,
                    DateTime.UtcNow,
                    queueName,
                    record.QueueOptions
                );
            }
            catch (Exception ex)
            {

            }
        }

        public async Task RemoveRecordAsync(Guid uniqueId)
        {
            var record = await _dbContext.PendingLogs.FindAsync(uniqueId);
            if (record != null)
            {
                _dbContext.PendingLogs.Remove(record);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
