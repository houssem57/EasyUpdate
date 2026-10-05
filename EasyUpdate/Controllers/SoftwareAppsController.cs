using EasyUpdate.Data;
using EasyUpdate.Data.Entities;
using EasyUpdate.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyUpdate.Controllers
{
    [Authorize]
    public class SoftwareAppsController : Controller
    {
        private readonly AppDbContext _context;

        public SoftwareAppsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SoftwareApps
        public async Task<IActionResult> Index()
        {
            var apps = await _context.SoftwareApps.OrderBy(a => a.Name).ToListAsync();
            return View(apps);
        }

        // GET: SoftwareApps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SoftwareApps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SoftwareAppViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var app = new SoftwareApp
            {
                Name = model.Name,
                AppFolderPath = model.AppFolderPath,
                DeployScriptPath = model.DeployScriptPath,
                RollbackScriptPath = model.RollbackScriptPath,
                CurrentVersion = model.CurrentVersion
            };

            _context.SoftwareApps.Add(app);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: SoftwareApps/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var app = await _context.SoftwareApps.FindAsync(id);
            if (app == null) return NotFound();

            var model = new SoftwareAppViewModel
            {
                Id = app.Id,
                Name = app.Name,
                AppFolderPath = app.AppFolderPath,
                DeployScriptPath = app.DeployScriptPath,
                RollbackScriptPath = app.RollbackScriptPath,
                CurrentVersion = app.CurrentVersion
            };

            return View(model);
        }

        // POST: SoftwareApps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SoftwareAppViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var app = await _context.SoftwareApps.FindAsync(id);
            if (app == null) return NotFound();

            app.Name = model.Name;
            app.AppFolderPath = model.AppFolderPath;
            app.DeployScriptPath = model.DeployScriptPath;
            app.RollbackScriptPath = model.RollbackScriptPath;
            app.CurrentVersion = model.CurrentVersion;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: SoftwareApps/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var app = await _context.SoftwareApps.FindAsync(id);
            if (app == null) return NotFound();
            return View(app);
        }

        // POST: SoftwareApps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var app = await _context.SoftwareApps.FindAsync(id);
            if (app == null) return NotFound();

            _context.SoftwareApps.Remove(app);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}