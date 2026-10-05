namespace EasyUpdate.Data.Entities
{
    public class SoftwareApp
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // The exact folder on the server that holds the live app.
        // The Windows Service backs this up to a temp folder before updating,
        // and restores it here automatically if the update fails.
        public string AppFolderPath { get; set; }

        // PowerShell/batch script that performs the deploy (e.g. stop service,
        // copy files, restart service). Receives package path and app folder path as arguments.
        public string DeployScriptPath { get; set; }

        // PowerShell/batch script that performs the rollback (e.g. stop service,
        // restore backup, restart service). Receives the temp backup path and app folder path as arguments.
        public string RollbackScriptPath { get; set; }

        public string CurrentVersion { get; set; }

        public ICollection<ScheduledUpdate> ScheduledUpdates { get; set; } = new List<ScheduledUpdate>();
    }
}