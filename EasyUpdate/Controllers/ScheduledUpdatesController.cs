using System.Security.Claims;
using EasyUpdate.Data;
using EasyUpdate.Data.Entities;
using EasyUpdate.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyUpdate.Controllers
{
    [Authorize]
    public class ScheduledUpdatesController : Controller
    {
        private readonly AppDbContext _context;

        public ScheduledUpdatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ScheduledUpdates (upcoming/active only)
        public async Task<IActionResult> Index()
        {
            var updates = await _context.ScheduledUpdates
                .Include(u => u.SoftwareApp)
                .Include(u => u.ScheduledByUser)
                .Where(u => u.Status == UpdateStatus.Pending || u.Status == UpdateStatus.Running)
                .OrderBy(u => u.ScheduledStart)
                .ToListAsync();

            return View(updates);
        }
        // GET: ScheduledUpdates/History
        public async Task<IActionResult> History()
        {
            var updates = await _context.ScheduledUpdates
                .Include(u => u.SoftwareApp)
                .Include(u => u.ScheduledByUser)
                .Where(u => u.Status == UpdateStatus.Completed
                         || u.Status == UpdateStatus.Failed
                         || u.Status == UpdateStatus.RolledBack
                         || u.Status == UpdateStatus.Cancelled)
                .OrderByDescending(u => u.ScheduledStart)
                .ToListAsync();

            return View(updates);
        }

        // GET: ScheduledUpdates/Create
        public async Task<IActionResult> Create()
        {
            var model = new ScheduledUpdateViewModel
            {
                ScheduledStart = DateTime.Today.AddDays(1).AddHours(19), // sensible default: tomorrow 7 PM
                AvailableApps = await GetAppOptions()
            };
            return View(model);
        }

        // POST: ScheduledUpdates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduledUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableApps = await GetAppOptions();
                return View(model);
            }

            var app = await _context.SoftwareApps.FindAsync(model.SoftwareAppId);
            if (app == null)
            {
                ModelState.AddModelError(nameof(model.SoftwareAppId), "Selected application no longer exists.");
                model.AvailableApps = await GetAppOptions();
                return View(model);
            }

            // Validate the package path exists and is reachable
            if (!Directory.Exists(model.PackagePath) && !System.IO.File.Exists(model.PackagePath))
            {
                ModelState.AddModelError(nameof(model.PackagePath),
                    "This path doesn't exist or isn't reachable from this machine. Double-check the path and permissions.");
                model.AvailableApps = await GetAppOptions();
                return View(model);
            }

            // Validate the app's own deploy/rollback scripts still exist (catches misconfigured SoftwareApp records early too)
            if (!System.IO.File.Exists(app.DeployScriptPath))
            {
                ModelState.AddModelError(string.Empty,
                    $"Deploy script for '{app.Name}' was not found at '{app.DeployScriptPath}'. Fix this in the application settings before scheduling.");
                model.AvailableApps = await GetAppOptions();
                return View(model);
            }

            if (!System.IO.File.Exists(app.RollbackScriptPath))
            {
                ModelState.AddModelError(string.Empty,
                    $"Rollback script for '{app.Name}' was not found at '{app.RollbackScriptPath}'. Fix this in the application settings before scheduling.");
                model.AvailableApps = await GetAppOptions();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var update = new ScheduledUpdate
            {
                SoftwareAppId = model.SoftwareAppId,
                TargetVersion = model.TargetVersion,
                PackagePath = model.PackagePath,
                ScheduledStart = model.ScheduledStart,
                Status = UpdateStatus.Pending,
                ScheduledByUserId = userId
            };

            _context.ScheduledUpdates.Add(update);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ScheduledUpdates/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var update = await _context.ScheduledUpdates
                .Include(u => u.SoftwareApp)
                .Include(u => u.ScheduledByUser)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (update == null) return NotFound();
            return View(update);
        }

        // POST: ScheduledUpdates/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var update = await _context.ScheduledUpdates.FindAsync(id);
            if (update == null) return NotFound();

            // only allow cancelling if it hasn't started yet
            if (update.Status == UpdateStatus.Pending)
            {
                update.Status = UpdateStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SoftwareAppOption>> GetAppOptions()
        {
            return await _context.SoftwareApps
                .OrderBy(a => a.Name)
                .Select(a => new SoftwareAppOption { Id = a.Id, Name = a.Name })
                .ToListAsync();
        }
    }
}