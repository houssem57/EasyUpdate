using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyUpdate.Controllers
{
    [Authorize]
    public class FileBrowserController : Controller
    {
        // GET: /FileBrowser/ListFolders?path=C:\Some\Folder&includeFiles=true&extensions=.ps1
        [HttpGet]
        public IActionResult ListFolders(string? path, bool includeFiles = false, string? extensions = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    var drives = DriveInfo.GetDrives()
                        .Where(d => d.IsReady)
                        .Select(d => new FolderItem { Name = d.Name, FullPath = d.Name })
                        .ToList();

                    return Json(new { currentPath = "", parentPath = (string?)null, folders = drives, files = new List<FolderItem>() });
                }

                // If the saved value is a file, start in the folder that contains it
                if (System.IO.File.Exists(path))
                    path = Path.GetDirectoryName(path)!;

                if (!Directory.Exists(path))
                    return BadRequest("Path does not exist.");

                var folders = Directory.GetDirectories(path)
                    .Select(d => new FolderItem { Name = Path.GetFileName(d), FullPath = d })
                    .OrderBy(f => f.Name)
                    .ToList();

                var files = new List<FolderItem>();
                if (includeFiles)
                {
                    var allowed = (extensions ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(e => e.ToLowerInvariant())
                        .ToList();

                    files = Directory.GetFiles(path)
                        .Where(f => allowed.Count == 0 || allowed.Contains(Path.GetExtension(f).ToLowerInvariant()))
                        .Select(f => new FolderItem { Name = Path.GetFileName(f), FullPath = f })
                        .OrderBy(f => f.Name)
                        .ToList();
                }

                var parent = Directory.GetParent(path)?.FullName;

                return Json(new { currentPath = path, parentPath = parent, folders, files });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, "Access denied to this folder.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading folder: {ex.Message}");
            }
        }

        public class FolderItem
        {
            public string Name { get; set; } = "";
            public string FullPath { get; set; } = "";
        }
    }
}