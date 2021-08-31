using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly INotyfService _notyf;

        public ProjectController(ApplicationDbContext db, INotyfService notyf)
        {
            _db = db;
            _notyf = notyf;
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

        public IActionResult Users(int id, string userfilter = null, int? rolefilter = null, string messageType = null, string message = null)
        {
            Project project = _db.Projects.Include(u => u.Creator).FirstOrDefault(u => u.Id == id);
            IEnumerable<UserProject> userProjects = _db.UsersProjects.Include(u => u.User).Include(r => r.ProjectRole).Where(p => p.ProjectId == id);
            IEnumerable<SelectListItem> roles = _db.ProjectRoles.Select((ProjectRole i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            // Filter the userProjects
            userProjects = HelperFunctions.FilterUserProjects(userProjects, userfilter, rolefilter);

            // Show a confirmation toast if needed
            HelperFunctions.ManageToastMessages(_notyf, messageType, message);

            // Prepare the VM for the view
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

            string message = "User updated successfully";
            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId, messageType = WC.MessageTypeSuccess, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserFromProject(UserProject userProject)
        {
            _db.UsersProjects.Remove(userProject);
            _db.SaveChanges();

            string message = "User deleted from the project";
            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId, messageType = WC.MessageTypeSuccess, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteProject(Project project)
        {
            // Remove all the UserProjects from the db
            _db.UsersProjects.RemoveRange(_db.UsersProjects.Where(up => up.ProjectId == project.Id));

            // Remove the project from the db
            _db.Projects.Remove(project);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpGet]
        [ActionName("Select")]
        public IActionResult SelectUser(int id)
        {
            IEnumerable<AppUser> users = _db.AppUsers;
            //TODO: extract in function
            IEnumerable<SelectListItem> roles = _db.ProjectRoles.Select((ProjectRole i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            SelectUserVM selectUserVM = new SelectUserVM()
            {
                Users = users,
                UserProject = new UserProject() { ProjectId = id },
                ProjectRoles = roles
            };

            return PartialView("_SelectUserModal", selectUserVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("AddUser")]
        public IActionResult AddUserToProject(UserProject userProject)
        {
            string message = "";

            try
            {
                // Success
                _db.UsersProjects.Add(userProject);
                _db.SaveChanges();

                message = "User added successfully to the project";
                return RedirectToAction(nameof(Users), new { id = userProject.ProjectId, messageType = WC.MessageTypeSuccess, message });
            }
            catch (DbUpdateException)
            {
                // User alredy present
                message = "The selected user is already included in this project";
                return RedirectToAction(nameof(Users), new { id = userProject.ProjectId, messageType = WC.MessageTypeError, message });
            }
            catch (Exception)
            {
                // Error
                message = "Something went wrong";
                return RedirectToAction(nameof(Users), new { id = userProject.ProjectId, messageType = WC.MessageTypeError, message });
            }

        }
    }
}
