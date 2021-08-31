using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
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

        public IActionResult Users(int id, string userfilter = null, int? rolefilter = null)
        {
            Project project = _db.Projects.Include(u => u.Creator).FirstOrDefault(u => u.Id == id);
            IEnumerable<UserProject> userProjects = _db.UsersProjects.Include(u => u.User).Include(r => r.ProjectRole).Where(p => p.ProjectId == id);
            IEnumerable<SelectListItem> roles = _db.ProjectRoles.Select((ProjectRole i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            if (!string.IsNullOrEmpty(userfilter))
            {
                // Filter by name and/or email
                userProjects = userProjects.Where(u => u.User.FullName.ToLower().Contains(userfilter.ToLower())
                || u.User.Email.ToLower().Contains(userfilter.ToLower()));
            }

            if (rolefilter != null && rolefilter > 0)
                // Filter by role
                userProjects = userProjects.Where(u => u.ProjectRole.Id == rolefilter);

            ProjectUsersVM projectUsersVM = new ProjectUsersVM()
            {
                Project = project,
                UserProjects = userProjects,
                ProjectRoles = roles
            };

            return View(projectUsersVM);
        }

        public IActionResult EditUserRole(string userId, int projectId)
        {
            UserProject userProject = _db.UsersProjects.Include(u => u.User).Include(u => u.ProjectRole).FirstOrDefault(u => u.UserId == userId && u.ProjectId == projectId);
            IEnumerable<SelectListItem> roles = _db.ProjectRoles.Select((ProjectRole i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            EditUserProjectVM editUserProjectVM = new EditUserProjectVM
            {
                Project = _db.Projects.Find(projectId),
                UserProject = userProject,
                ProjectRoles = roles
            };

            return View(editUserProjectVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EditUserRole")]
        public IActionResult EditUserRolePost(UserProject userProject)
        {
            _db.UsersProjects.Update(userProject);
            _db.SaveChanges();

            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserFromProject(UserProject userProject)
        {
            _db.UsersProjects.Remove(userProject);
            _db.SaveChanges();

            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Project project)
        {
            // Remove all the UserProjects from the db
            _db.UsersProjects.RemoveRange(_db.UsersProjects.Where(up => up.ProjectId == project.Id));

            // Remove the project from the db
            _db.Projects.Remove(project);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index), "Home");
        }
    }
}
