using System.Diagnostics;
using System.Text;
using EasyUpdate.Data;
using EasyUpdate.Data.Entities;

namespace EasyUpdate.UpdateService
{
    public class UpdateExecutor : IUpdateExecutor
    {
        private static readonly TimeSpan ScriptTimeout = TimeSpan.FromMinutes(30);

        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<UpdateExecutor> _logger;

        public UpdateExecutor(IEmailService emailService, IConfiguration config, ILogger<UpdateExecutor> logger)
        {
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        public async Task ExecuteUpdateAsync(ScheduledUpdate update, AppDbContext context, CancellationToken ct)
        {
            var app = update.SoftwareApp;
            var log = new StringBuilder();

            // Backups go to Update:BackupRoot if set, otherwise the system temp folder
            var backupRoot = _config["Update:BackupRoot"];
            if (string.IsNullOrWhiteSpace(backupRoot))
                backupRoot = Path.GetTempPath();

            string tempFolder = Path.Combine(backupRoot, $"update_backup_{app.Id}_{DateTime.Now:yyyyMMdd_HHmmss}");

            update.Status = UpdateStatus.Running;
            update.ExecutedAt = DateTime.Now;
            await context.SaveChangesAsync(ct);

            bool backupCreated = false;

            try
            {
                // 1. Copy the current version into the temp folder
                log.AppendLine($"[{DateTime.Now:T}] Backing up '{app.AppFolderPath}' to '{tempFolder}'");
                CopyDirectory(app.AppFolderPath, tempFolder);
                backupCreated = true;

                // 2. Run the deploy script: args are <package path> <app folder>
                log.AppendLine($"[{DateTime.Now:T}] Running deploy script '{app.DeployScriptPath}'");
                var deploy = await RunScriptAsync(app.DeployScriptPath, update.PackagePath, app.AppFolderPath);
                log.AppendLine(deploy.Output);

                if (!deploy.Success)
                    throw new Exception($"Deploy script failed (exit code {deploy.ExitCode}).");

                // 3. Success: remove the backup
                log.AppendLine($"[{DateTime.Now:T}] Update succeeded, deleting backup");
                DeleteDirectorySafe(tempFolder);

                update.Status = UpdateStatus.Completed;
                update.CompletedAt = DateTime.Now;
                update.FailureReason = null;
                app.CurrentVersion = update.TargetVersion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update {Id} failed", update.Id);
                update.FailureReason = ex.Message;
                log.AppendLine($"[{DateTime.Now:T}] FAILED: {ex.Message}");

                if (!backupCreated)
                {
                    // The app folder was never touched, so there is nothing to roll back
                    DeleteDirectorySafe(tempFolder);
                    update.Status = UpdateStatus.Failed;
                    update.FailureReason += " The application was not changed.";
                }
                else
                {
                    await RollbackAsync(update, tempFolder, log);
                }

                update.CompletedAt = DateTime.Now;
            }

            update.LogOutput = log.ToString();
            await context.SaveChangesAsync(ct);

            if (update.Status != UpdateStatus.Completed)
            {
                try
                {
                    await _emailService.SendUpdateFailureEmail(update);
                }
                catch (Exception emailEx)
                {
                    // A broken mail server must never break the update flow
                    _logger.LogError(emailEx, "Could not send failure email for update {Id}", update.Id);
                }
            }
        }

        private async Task RollbackAsync(ScheduledUpdate update, string tempFolder, StringBuilder log)
        {
            var app = update.SoftwareApp;

            try
            {
                // Rollback script args: <backup folder> <app folder>
                log.AppendLine($"[{DateTime.Now:T}] Running rollback script '{app.RollbackScriptPath}'");
                var rollback = await RunScriptAsync(app.RollbackScriptPath, tempFolder, app.AppFolderPath);
                log.AppendLine(rollback.Output);

                if (!rollback.Success)
                    throw new Exception($"Rollback script failed (exit code {rollback.ExitCode}).");

                DeleteDirectorySafe(tempFolder);
                update.Status = UpdateStatus.RolledBack;
                log.AppendLine($"[{DateTime.Now:T}] Rollback completed, backup deleted");
            }
            catch (Exception rollbackEx)
            {
                // Keep the backup: it is the only good copy of the app left
                _logger.LogCritical(rollbackEx, "ROLLBACK FAILED for update {Id}. Backup kept at {Temp}", update.Id, tempFolder);
                update.Status = UpdateStatus.Failed;
                update.FailureReason += $" ROLLBACK ALSO FAILED: {rollbackEx.Message} The last working copy is preserved at: {tempFolder}";
                log.AppendLine($"[{DateTime.Now:T}] ROLLBACK FAILED: {rollbackEx.Message}");
            }
        }

        private static async Task<(bool Success, int ExitCode, string Output)> RunScriptAsync(
            string scriptPath, string arg1, string arg2)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // ArgumentList handles spaces and quotes in paths safely
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-NonInteractive");
            psi.ArgumentList.Add("-ExecutionPolicy");
            psi.ArgumentList.Add("Bypass");
            psi.ArgumentList.Add("-File");
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add(arg1);
            psi.ArgumentList.Add(arg2);

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Could not start powershell.exe");

            // Read both streams asynchronously to avoid a deadlock on large output
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var timeout = new CancellationTokenSource(ScriptTimeout);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                return (false, -1, $"Script timed out after {ScriptTimeout.TotalMinutes} minutes and was stopped.");
            }

            var output = await stdoutTask + await stderrTask;
            return (process.ExitCode == 0, process.ExitCode, output);
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: true);

            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectory(dir, Path.Combine(target, Path.GetFileName(dir)));
        }

        private void DeleteDirectorySafe(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete backup folder {Path}", path);
            }
        }
    }
}