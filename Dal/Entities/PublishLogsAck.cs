using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("publish_logs_ack", Schema = "rabbitmq")]
public partial class PublishLogsAck
{
    [Key]
    [Column("unique_id")]
    public Guid UniqueId { get; set; }

    [Column("consume_message_id")]
    public Guid? ConsumeMessageId { get; set; }

    [Column("exchange_name", TypeName = "character varying")]
    public string? ExchangeName { get; set; }

    [Column("queue_name", TypeName = "character varying")]
    public string QueueName { get; set; } = null!;

    [Column("message_body", TypeName = "jsonb")]
    public string MessageBody { get; set; } = null!;

    [Column("queue_options", TypeName = "jsonb")]
    public string? QueueOptions { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("publish_at", TypeName = "timestamp without time zone")]
    public DateTime? PublishAt { get; set; }

    [Column("fin_year")]
    public short? FinYear { get; set; }
}
