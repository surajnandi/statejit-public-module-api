using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("consume_logs", Schema = "rabbitmq")]
public partial class ConsumeLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("message_id", TypeName = "character varying")]
    public string? MessageId { get; set; }

    [Column("queue_name", TypeName = "character varying")]
    public string? QueueName { get; set; }

    [Column("exchange_name", TypeName = "character varying")]
    public string? ExchangeName { get; set; }

    [Column("raouting_key", TypeName = "character varying")]
    public string? RaoutingKey { get; set; }

    [Column("message_body")]
    public string? MessageBody { get; set; }

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
