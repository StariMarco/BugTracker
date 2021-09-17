using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectRoleRepository _projetRoleRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IAppUserRepository _appUserRepo;
        private readonly IUserProjectRepository _userProjectRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _web;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public ProjectController(IProjectRoleRepository projetRoleRepo, IProjectRepository projectRepo, IAppUserRepository appUserRepo,
            IUserProjectRepository userProjectRepo, ITicketRepository ticketRepo, INotyfService notyf,
            IWebHostEnvironment web, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _projetRoleRepo = projetRoleRepo;
            _projectRepo = projectRepo;
            _appUserRepo = appUserRepo;
            _userProjectRepo = userProjectRepo;
            _ticketRepo = ticketRepo;
            _notyf = notyf;
            _web = web;
            _userManager = userManager;
            _configuration = configuration;
        }

        public IActionResult Settings(int projectId)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            bool isAdmin = User.IsInRole(WC.AdminRole);

            UserProject userProject = IdentityHelper.GetUserProject(_userProjectRepo, isAdmin, projectId, userId, includeProperties: "Project,Project.Creator");

            // Can edit if admin or project manager
            ViewBag.CanUserEdit = isAdmin || userProject.ProjectRoleId == WC.ProjectManagerId;

            return View(userProject.Project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Settings")]
        public IActionResult SettingsPost(Project project)
        {
            try
            {
                bool canUserEdit = bool.Parse(Request.Form["canUserEdit"]);
                if (!canUserEdit) throw new UnauthorizedAccessException();

                _projectRepo.Update(project);
                _projectRepo.Save();

                string message = "The project was updated successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (UnauthorizedAccessException)
            {
                string message = "You do not have the permission to edit this project";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), nameof(Ticket), new { projectId = project.Id });
        }

        public IActionResult Users(int projectId, string userfilter = null, int? rolefilter = null)
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            bool isAdmin = User.IsInRole(WC.AdminRole);

            Project project = _projectRepo.FirstOrDefault(u => u.Id == projectId, includeProperties: "Creator");
            IEnumerable<UserProject> userProjects = _userProjectRepo.GetAll(p => p.ProjectId == projectId, includeProperties: "User,ProjectRole");
            IEnumerable<SelectListItem> roles = _projetRoleRepo.GetAllSelectListItems();

            // Filter the userProjects
            userProjects = HelperFunctions.FilterUserProjects(userProjects, userfilter, rolefilter);

            // Check if current user is manager
            bool isManager = userProjects.Where(o => o.UserId == userId && o.ProjectRoleId == WC.ProjectManagerId).Count() > 0;

            // Prepare the VM for the view
            ProjectUsersVM projectUsersVM = new ProjectUsersVM()
            {
                Project = project,
                UserProjects = userProjects,
                ProjectRoles = roles,
                // Can edit or add user if admin or project manager
                CanEditUsers = isAdmin || isManager
            };

            return View(projectUsersVM);
        }

        public IActionResult EditUserRole(string userId, int projectId, bool canEdit = false)
        {
            if (!canEdit) return RedirectToAction(nameof(Users), new { projectId = projectId });

            UserProject userProject = _userProjectRepo.FirstOrDefault(u => u.UserId == userId && u.ProjectId == projectId, includeProperties: "User,ProjectRole");
            IEnumerable<SelectListItem> roles = _projetRoleRepo.GetAllSelectListItems();

            EditUserProjectVM editUserProjectVM = new EditUserProjectVM
            {
                Project = _projectRepo.Find(projectId),
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
            try
            {
                _userProjectRepo.Update(userProject);
                _userProjectRepo.Save();

                string message = "User updated successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Users), new { projectId = userProject.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserFromProject(UserProject userProject)
        {
            try
            {
                _userProjectRepo.Remove(userProject);
                _userProjectRepo.Save();

                string message = "User deleted from the project";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Users), new { projectId = userProject.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteProject(Project project, bool canDelete = false)
        {
            try
            {
                if (!canDelete) throw new UnauthorizedAccessException();
                // Remove the project from the db (with all the tickets, comments ecc...)
                _projectRepo.RemoveProject(_configuration, _ticketRepo, project);
                _projectRepo.Save();

                string message = "The project was deleted successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (UnauthorizedAccessException)
            {
                string message = "You do not have the permission to delete this project";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpGet]
        [ActionName("Select")]
        public IActionResult SelectUser(int id, bool canEdit = false)
        {
            IEnumerable<AppUser> users = _appUserRepo.GetAll();
            IEnumerable<SelectListItem> roles = _projetRoleRepo.GetAllSelectListItems();

            SelectUserProjectVM selectUserVM = new SelectUserProjectVM()
            {
                Users = users,
                UserProject = new UserProject() { ProjectId = id },
                ProjectRoles = roles,
                CanAddUser = canEdit
            };

            return PartialView("_SelectUserProjectModal", selectUserVM);

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
                _userProjectRepo.Add(userProject);
                _userProjectRepo.Save();

                message = "User added successfully to the project";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (DbUpdateException)
            {
                // User alredy present
                message = "The selected user is already included in this project";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
            }
            catch (Exception)
            {
                // Error
                message = "Something went wrong";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Users), new { projectId = userProject.ProjectId });
        }
    }
}
