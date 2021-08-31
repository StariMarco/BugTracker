using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using BugTracker.Data.Repository.IRepository;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;

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

        public HomeController(ILogger<HomeController> logger, IProjectRepository projectRepo, IAppUserRepository appUserRepo, IUserProjectRepository userProjectRepo, INotyfService notyf)
        {
            _logger = logger;
            _projectRepo = projectRepo;
            _appUserRepo = appUserRepo;
            _userProjectRepo = userProjectRepo;
            _notyf = notyf;
        }

        public IActionResult Index()
        {
            List<Project> projects = _projectRepo.GetAll().ToList() ?? new List<Project>();

            return View(projects);
        }

        public IActionResult Create()
        {
            //TODO: Change this to a modal user selector
            IEnumerable<SelectListItem> users = _appUserRepo.GetAll().Select((AppUser i) => new SelectListItem
            {
                Text = i.FullName + " - " + i.Email,
                Value = i.Id.ToString()
            });

            ViewBag.UserList = users;

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

                // Add the project manager
                UserProject projectManager = new UserProject()
                {
                    UserId = project.ProjectManagerId,
                    ProjectId = project.Id,
                    ProjectRoleId = WC.ProjectManagerId
                };
                _userProjectRepo.Add(projectManager);

                // Add the creator as a project manager
                if (project.ProjectManagerId != project.CreatorId)
                {
                    UserProject creator = new UserProject()
                    {
                        UserId = project.CreatorId,
                        ProjectId = project.Id,
                        ProjectRoleId = WC.ProjectManagerId
                    };
                    _userProjectRepo.Add(creator);
                }
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
