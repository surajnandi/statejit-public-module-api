using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sjam.Dal.Entities;

[Table("otp_request", Schema = "agency")]
public partial class OtpRequest
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("mobile_no")]
    [StringLength(10)]
    public string? MobileNo { get; set; }

    [Column("email_id", TypeName = "character varying")]
    public string? EmailId { get; set; }

    [Column("captcha_id")]
    public long? CaptchaId { get; set; }

    [Column("captcha_code", TypeName = "character varying")]
    public string? CaptchaCode { get; set; }

    [Column("otp_id")]
    public long? OtpId { get; set; }

    [Column("otp_code", TypeName = "character varying")]
    public string? OtpCode { get; set; }

    [Column("otp_hash_code")]
    public string? OtpHashCode { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("expiry_at", TypeName = "timestamp without time zone")]
    public DateTime? ExpiryAt { get; set; }

    [Column("is_verified")]
    public bool IsVerified { get; set; }

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("session_id")]
    public string? SessionId { get; set; }
}
