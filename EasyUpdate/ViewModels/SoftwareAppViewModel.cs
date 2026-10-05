using System.ComponentModel.DataAnnotations;

namespace EasyUpdate.ViewModels
{
    public class SoftwareAppViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Application Name")]
        public string Name { get; set; }

        [Required, Display(Name = "Application Folder Path")]
        public string AppFolderPath { get; set; }

        [Required, Display(Name = "Deploy Script Path")]
        public string DeployScriptPath { get; set; }

        [Required, Display(Name = "Rollback Script Path")]
        public string RollbackScriptPath { get; set; }

        [Display(Name = "Current Version")]
        public string CurrentVersion { get; set; }
    }
}
