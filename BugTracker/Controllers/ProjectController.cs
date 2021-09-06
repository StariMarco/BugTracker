using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;
using BugTracker.Data;
using BugTracker.Data.Repository.IRepository;
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
        private readonly IProjectRoleRepository _projetRoleRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IAppUserRepository _appUserRepo;
        private readonly IUserProjectRepository _userProjectRepo;
        private readonly INotyfService _notyf;

        public ProjectController(IProjectRoleRepository projetRoleRepo, IProjectRepository projectRepo, IAppUserRepository appUserRepo, IUserProjectRepository userProjectRepo, INotyfService notyf)
        {
            _projetRoleRepo = projetRoleRepo;
            _projectRepo = projectRepo;
            _appUserRepo = appUserRepo;
            _userProjectRepo = userProjectRepo;
            _notyf = notyf;
        }

        public IActionResult Settings(int id)
        {
            Project project = _projectRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Creator");

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Settings")]
        public IActionResult SettingsPost(Project project)
        {
            try
            {
                _projectRepo.Update(project);
                _projectRepo.Save();

                string message = "The project was updated successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), nameof(Ticket), new { id = project.Id });
        }

        public IActionResult Users(int id, string userfilter = null, int? rolefilter = null)
        {
            Project project = _projectRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Creator");
            IEnumerable<UserProject> userProjects = _userProjectRepo.GetAll(p => p.ProjectId == id, includeProperties: "User,ProjectRole");
            IEnumerable<SelectListItem> roles = _projetRoleRepo.GetAllSelectListItems();

            // Filter the userProjects
            userProjects = HelperFunctions.FilterUserProjects(userProjects, userfilter, rolefilter);

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

            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId });
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

            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteProject(Project project)
        {
            try
            {
                // Remove all the UserProjects from the db
                _userProjectRepo.RemoveRange(_userProjectRepo.GetAll(up => up.ProjectId == project.Id));

                // Remove the project from the db
                _projectRepo.Remove(project);
                _projectRepo.Save();

                string message = "The project was deleted successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpGet]
        [ActionName("Select")]
        public IActionResult SelectUser(int id)
        {
            IEnumerable<AppUser> users = _appUserRepo.GetAll();
            IEnumerable<SelectListItem> roles = _projetRoleRepo.GetAllSelectListItems();

            SelectUserProjectVM selectUserVM = new SelectUserProjectVM()
            {
                Users = users,
                UserProject = new UserProject() { ProjectId = id },
                ProjectRoles = roles
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

            return RedirectToAction(nameof(Users), new { id = userProject.ProjectId });
        }
    }
}
