using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProjectController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /<controller>/
        public IActionResult Index(int id)
        {
            Project project = _db.Projects.Find(id);

            return View(project);
        }

        public IActionResult Settings(int id)
        {
            Project project = _db.Projects.Include(u => u.Creator).FirstOrDefault(u => u.Id == id);

            IEnumerable<SelectListItem> users = _db.AppUsers.Select((AppUser i) => new SelectListItem
            {
                Text = i.FullName + " - " + i.Email,
                Selected = i.Id == project.ProjectManagerId,
                Value = i.Id.ToString()
            });

            ViewBag.UserList = users;

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Settings")]
        public IActionResult SettingsPost(Project project)
        {
            _db.Projects.Update(project);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = project.Id });
        }
    }
}
