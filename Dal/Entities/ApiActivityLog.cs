using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("api_activity_log", Schema = "audit")]
public partial class ApiActivityLog
{
    [Key]
    [Column("log_id")]
    public long LogId { get; set; }

    [Column("api_controller")]
    public string? ApiController { get; set; }

    [Column("api_method")]
    public string? ApiMethod { get; set; }

    [Column("api_endpoint")]
    public string? ApiEndpoint { get; set; }

    [Column("response_status_code")]
    public int? ResponseStatusCode { get; set; }

    [Column("request_data", TypeName = "jsonb")]
    public string? RequestData { get; set; }

    [Column("response_data", TypeName = "jsonb")]
    public string? ResponseData { get; set; }

    [Column("error_details", TypeName = "jsonb")]
    public string? ErrorDetails { get; set; }

    [Column("user_details", TypeName = "jsonb")]
    public string? UserDetails { get; set; }

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("fin_year")]
    public long? FinYear { get; set; }

    [Column("other_details", TypeName = "jsonb")]
    public string? OtherDetails { get; set; }
}
