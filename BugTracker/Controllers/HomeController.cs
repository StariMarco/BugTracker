using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Data.Repository.IRepository;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProjectRepository _projectRepo;
        private readonly IAppUserRepository _appUserRepo;
        private readonly IUserProjectRepository _userProjectRepo;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IProjectRepository projectRepo, IAppUserRepository appUserRepo,
            IUserProjectRepository userProjectRepo, INotyfService notyf, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _projectRepo = projectRepo;
            _appUserRepo = appUserRepo;
            _userProjectRepo = userProjectRepo;
            _notyf = notyf;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            List<UserProject> userProjects = new List<UserProject>();

            if (User.IsInRole(WC.AdminRole))
            {
                var projects = _projectRepo.GetAll().ToList();

                foreach (var obj in projects)
                {
                    userProjects.Add(new UserProject
                    {
                        Project = obj,
                        ProjectId = obj.Id,
                        UserId = userId,
                        ProjectRoleId = WC.ProjectManagerId
                    });
                }

            }
            else
                userProjects = _userProjectRepo.GetAll(o => o.UserId == userId, includeProperties: "Project").ToList() ?? new List<UserProject>();

            return View(userProjects);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public IActionResult CreatePost(Project project)
        {
            try
            {
                // Add project to db
                project.CreatedAt = DateTime.Now;
                _projectRepo.Add(project);
                _projectRepo.Save();

                // Add the creator as a project manager
                UserProject creator = new UserProject()
                {
                    UserId = project.CreatorId,
                    ProjectId = project.Id,
                    ProjectRoleId = WC.ProjectManagerId
                };
                _userProjectRepo.Add(creator);
                _projectRepo.Save();

                string message = "The project was created successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
