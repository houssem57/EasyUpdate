using System;
using System.Collections.Generic;
using System.Text;

namespace EasyUpdate.Data.Entities
{
    public class ScheduledUpdate
    {
        public int Id { get; set; }

        public int SoftwareAppId { get; set; }
        public SoftwareApp SoftwareApp { get; set; }

        public string TargetVersion { get; set; }
        public string PackagePath { get; set; }

        public DateTime ScheduledStart { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public UpdateStatus Status { get; set; } = UpdateStatus.Pending;
        public string? FailureReason { get; set; }
        public string? LogOutput { get; set; }

        public string ScheduledByUserId { get; set; }
        public Users ScheduledByUser { get; set; }
    }
}
