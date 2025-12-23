using ReSys.Shop.Core.Domain.Auditing;

namespace ReSys.Shop.Core.Feature.Admin.SystemAdmin;

public static partial class SystemModule
{
    public static class Models
    {
        public sealed class AuditLogItem
        {
            public Guid Id { get; set; }
            public string? EntityName { get; set; }
            public string? EntityId { get; set; }
            public string? Action { get; set; }
            public DateTimeOffset Timestamp { get; set; }
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserEmail { get; set; }
            public string? IpAddress { get; set; }
            public string? UserAgent { get; set; }
            public string? OldValues { get; set; }
            public string? NewValues { get; set; }
            public string? ChangedProperties { get; set; }
            public string? Reason { get; set; }
            public AuditSeverity Severity { get; set; }
        }
    }
}