using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using sjam.Dal.Entities;

namespace sjam.Dal;

public partial class EFContext : DbContext
{
    public EFContext(DbContextOptions<EFContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApiActivityLog> ApiActivityLogs { get; set; }

    public virtual DbSet<ConfigMaster> ConfigMasters { get; set; }

    public virtual DbSet<ConsumeFailedLog> ConsumeFailedLogs { get; set; }

    public virtual DbSet<ConsumeLog> ConsumeLogs { get; set; }

    public virtual DbSet<ConsumeLogsAck> ConsumeLogsAcks { get; set; }

    public virtual DbSet<FinancialYear> FinancialYears { get; set; }

    public virtual DbSet<OtpRequest> OtpRequests { get; set; }

    public virtual DbSet<PendingLog> PendingLogs { get; set; }

    public virtual DbSet<PublishFailedLog> PublishFailedLogs { get; set; }

    public virtual DbSet<PublishLog> PublishLogs { get; set; }

    public virtual DbSet<PublishLogsAck> PublishLogsAcks { get; set; }

    public virtual DbSet<QueuesMaster> QueuesMasters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("api_activity_log_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<ConfigMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("config_master_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Message).HasDefaultValueSql("'Service is temporarily unavailable due to scheduled maintenance. Please try again later!'::text");
        });

        modelBuilder.Entity<ConsumeFailedLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consume_failed_logs_pkey");

            entity.Property(e => e.ActionStatus).HasDefaultValueSql("'PENDING'::character varying");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
            entity.Property(e => e.IsRedelivered).HasDefaultValue(false);
        });

        modelBuilder.Entity<ConsumeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consume_logs_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
        });

        modelBuilder.Entity<ConsumeLogsAck>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("consume_logs_ack_pkey");

            entity.Property(e => e.UniqueId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
        });

        modelBuilder.Entity<FinancialYear>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("financial_year_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'SYSTEM'::character varying");
            entity.Property(e => e.IsActive).HasDefaultValue(false);
        });

        modelBuilder.Entity<OtpRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("otp_request_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
        });

        modelBuilder.Entity<PendingLog>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("pending_logs_pkey");

            entity.Property(e => e.UniqueId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
        });

        modelBuilder.Entity<PublishFailedLog>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("publish_failed_logs_pkey");

            entity.Property(e => e.UniqueId).ValueGeneratedNever();
            entity.Property(e => e.ActionStatus).HasDefaultValueSql("'PENDING'::character varying");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
        });

        modelBuilder.Entity<PublishLog>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("publish_logs_pkey");

            entity.Property(e => e.UniqueId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
        });

        modelBuilder.Entity<PublishLogsAck>(entity =>
        {
            entity.HasKey(e => e.UniqueId).HasName("publish_logs_ack_pkey");

            entity.Property(e => e.UniqueId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
        });

        modelBuilder.Entity<QueuesMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("queues_master_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.FinYear).HasDefaultValueSql("get_active_fin_year()");
            entity.Property(e => e.Status)
                .HasDefaultValue((short)1)
                .HasComment("Status: 1 - Active, 0 - Inactive");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
