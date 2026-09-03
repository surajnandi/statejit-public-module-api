namespace sjam.RabbitMQ.Models.RabbitMQModel
{
    public class ConsumeLogFailedModel
    {
        public long Id { get; set; }
        public Guid? MessageId { get; set; }
        public string? QueueName { get; set; }
        public string? ExchangeName { get; set; }
        public string? RaoutingKey { get; set; }
        public string? MessageBody { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public string? FailedType { get; set; }
        public string? FailedMessage { get; set; }
        public DateTime FailedAt { get; set; }
        public string ActionStatus { get; set; } = "PENDING";
        public DateTime? ResolvedAt { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsRedelivered { get; set; } = false;
    }
}
