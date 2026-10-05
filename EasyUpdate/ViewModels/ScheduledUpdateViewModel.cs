using System.ComponentModel.DataAnnotations;

namespace EasyUpdate.ViewModels
{
    public class ScheduledUpdateViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Application")]
        public int SoftwareAppId { get; set; }

        [Required, Display(Name = "Target Version")]
        public string TargetVersion { get; set; }

        [Required, Display(Name = "Package Path")]
        public string PackagePath { get; set; }

        [Required, Display(Name = "Scheduled Start")]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledStart { get; set; }

        // populated for the dropdown, not submitted by the user
        public List<SoftwareAppOption>? AvailableApps { get; set; }
    }

    public class SoftwareAppOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}