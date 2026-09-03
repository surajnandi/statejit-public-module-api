using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("financial_year", Schema = "master")]
public partial class FinancialYear
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fin_year_short")]
    public short? FinYearShort { get; set; }

    [Column("fin_year_long", TypeName = "character varying")]
    public string? FinYearLong { get; set; }

    [Column("current_fin_year")]
    public short? CurrentFinYear { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by", TypeName = "character varying")]
    public string? CreatedBy { get; set; }
}
