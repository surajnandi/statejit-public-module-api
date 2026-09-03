namespace sjam.RabbitMQ.Models.RabbitMQModel
{
    public class ConsumedLogModel
    {
        public Guid? MessageId { get; set; }
        public string QueueName { get; set; } = null!;
        public string? ExchangeName { get; set; }
        public string? RaoutingKey { get; set; }
        public string MessageBody { get; set; }
        public string? Status { get; set; }
        public string? ActionStatus { get; set; }
        public string? FailedMessage { get; set; }
        public string? FailedType { get; set; }
        public DateTime FailedAt { get; set; } = DateTime.UtcNow;
        public DateTime ConsumedAt { get; set; } = DateTime.UtcNow;
        public bool IsRedelivered { get; set; } = false;

        public void Reset()
        {
            MessageId = null;
            ExchangeName = null;
            RaoutingKey = null;
            MessageBody = null;
            Status = null;
            ActionStatus = null;
            FailedMessage = null;
            FailedType = null;
            ConsumedAt = DateTime.UtcNow;
            FailedAt = DateTime.UtcNow;
            // QueueName is intentionally not reset
        }
    }
}
