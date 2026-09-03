using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("consume_failed_logs", Schema = "rabbitmq")]
public partial class ConsumeFailedLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("message_id")]
    public Guid? MessageId { get; set; }

    [Column("queue_name", TypeName = "character varying")]
    public string? QueueName { get; set; }

    [Column("exchange_name", TypeName = "character varying")]
    public string? ExchangeName { get; set; }

    [Column("routing_key", TypeName = "character varying")]
    public string? RoutingKey { get; set; }

    [Column("message_body")]
    public string? MessageBody { get; set; }

    [Column("consumed_at", TypeName = "timestamp without time zone")]
    public DateTime? ConsumedAt { get; set; }

    [Column("failed_type", TypeName = "character varying")]
    public string? FailedType { get; set; }

    [Column("failed_message")]
    public string? FailedMessage { get; set; }

    [Column("failed_at", TypeName = "timestamp without time zone")]
    public DateTime FailedAt { get; set; }

    [Column("action_status", TypeName = "character varying")]
    public string ActionStatus { get; set; } = null!;

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_redelivered")]
    public bool IsRedelivered { get; set; }

    [Column("fin_year")]
    public short? FinYear { get; set; }
}
