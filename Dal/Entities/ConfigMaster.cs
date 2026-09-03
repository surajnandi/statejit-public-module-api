using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("config_master", Schema = "master")]
public partial class ConfigMaster
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("controller_name")]
    public string? ControllerName { get; set; }

    [Column("action_name")]
    public string? ActionName { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("scheduled_start", TypeName = "timestamp without time zone")]
    public DateTime? ScheduledStart { get; set; }

    [Column("scheduled_end", TypeName = "timestamp without time zone")]
    public DateTime? ScheduledEnd { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by", TypeName = "character varying")]
    public string? CreatedBy { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by", TypeName = "character varying")]
    public string? UpdatedBy { get; set; }

    [Column("fin_year")]
    public short? FinYear { get; set; }
}
