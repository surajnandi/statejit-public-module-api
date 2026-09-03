using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("queues_master", Schema = "rabbitmq")]
[Index("Identifier", Name = "queues_master_identifier_key", IsUnique = true)]
public partial class QueuesMaster
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("queue_name", TypeName = "character varying")]
    public string QueueName { get; set; } = null!;

    [Column("identifier", TypeName = "character varying")]
    public string Identifier { get; set; } = null!;

    /// <summary>
    /// Status: 1 - Active, 0 - Inactive
    /// </summary>
    [Column("status")]
    public short Status { get; set; }

    [Column("exchange_name", TypeName = "character varying")]
    public string? ExchangeName { get; set; }

    [Column("producer", TypeName = "character varying")]
    public string? Producer { get; set; }

    [Column("consumer", TypeName = "character varying")]
    public string? Consumer { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("fin_year")]
    public short? FinYear { get; set; }
}
