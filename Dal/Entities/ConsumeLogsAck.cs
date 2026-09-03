using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("consume_logs_ack", Schema = "rabbitmq")]
public partial class ConsumeLogsAck
{
    [Key]
    [Column("unique_id")]
    public Guid UniqueId { get; set; }

    [Column("message_id")]
    public Guid? MessageId { get; set; }

    [Column("published_message_id")]
    public Guid? PublishedMessageId { get; set; }

    [Column("exchange_name", TypeName = "character varying")]
    public string? ExchangeName { get; set; }

    [Column("queue_name", TypeName = "character varying")]
    public string QueueName { get; set; } = null!;

    [Column("raouting_key", TypeName = "character varying")]
    public string? RaoutingKey { get; set; }

    [Column("message_body", TypeName = "jsonb")]
    public string MessageBody { get; set; } = null!;

    [Column("queue_options", TypeName = "jsonb")]
    public string? QueueOptions { get; set; }

    [Column("consumed_at", TypeName = "timestamp without time zone")]
    public DateTime? ConsumedAt { get; set; }

    [Column("status", TypeName = "character varying")]
    public string? Status { get; set; }

    [Column("error_messages")]
    public string? ErrorMessages { get; set; }

    [Column("error_type", TypeName = "character varying")]
    public string? ErrorType { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("fin_year")]
    public short? FinYear { get; set; }
}
