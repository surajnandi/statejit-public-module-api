namespace sjam.Models
{
    public sealed class ApiConfigMaster
    {
        public long? Id { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public bool? IsActive { get; set; }
        public string? Message { get; set; }
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public int? FinYear { get; set; }
    }
}
