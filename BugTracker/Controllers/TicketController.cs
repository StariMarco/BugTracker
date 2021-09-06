using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;
using BugTracker.Data;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IProjectRepository _projectRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _web;

        public TicketController(ApplicationDbContext db, IProjectRepository projectRepo, ITicketRepository ticketRepo, INotyfService notyf, IWebHostEnvironment web)
        {
            _db = db;
            _projectRepo = projectRepo;
            _ticketRepo = ticketRepo;
            _notyf = notyf;
            _web = web;
        }

        public IActionResult Index(int id, string userfilter = null, int? statusfilter = null, int? typefilter = null,
            string messageType = null, string message = null)
        {
            // Manage toasts
            if (messageType != null)
            {
                HelperFunctions.ManageToastMessages(_notyf, messageType, message);
                return RedirectToAction(nameof(Index), new { id = id, userfilter, statusfilter });
            }

            Project project = _projectRepo.Find(id);
            IEnumerable<Ticket> tickets = _ticketRepo.GetAll(t => t.ProjectId == project.Id, includeProperties: "Reporter,Developer,Reviewer,Status,Priority,Type", orderBy: (q) => q.OrderByDescending(t => t.CreatedAt));
            IEnumerable<SelectListItem> statuses = _ticketRepo.GetAllStatuses();
            IEnumerable<SelectListItem> types = _ticketRepo.GetAllTypes();

            // Filter the tickets
            tickets = HelperFunctions.FilterTickets(tickets, userfilter, typefilter, statusfilter);

            TicketIndexVM ticketIndexVM = new TicketIndexVM
            {
                Project = project,
                Tickets = tickets,
                Statuses = statuses,
                Types = types
            };

            return View(ticketIndexVM);
        }

        public IActionResult Create(int id)
        {
            Project project = _projectRepo.Find(id);
            IEnumerable<SelectListItem> types = _ticketRepo.GetAllTypes();
            IEnumerable<SelectListItem> priorities = _ticketRepo.GetAllPriorities();

            CreateTicketVM createTicketVM = new CreateTicketVM
            {
                Project = project,
                Ticket = new Ticket(),
                Types = types,
                Priorities = priorities
            };

            return View(createTicketVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public IActionResult CreatePost(Ticket ticket)
        {
            ticket.CreatedAt = DateTime.Now;
            ticket.StatusId = WC.StatusToDo;

            try
            {
                _db.Tickets.Add(ticket);
                _db.SaveChanges();

                string message = "The ticket was created successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), new { id = ticket.ProjectId });
        }

        // TODO: Delete this
        public IActionResult Details(int projectId, int ticketId)
        {
            Project project = _projectRepo.Find(projectId);
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Reporter,Developer,Reviewer,Status,Priority,Type");

            TicketDetailsVM ticketDetailsVM = new TicketDetailsVM
            {
                Project = project,
                Ticket = ticket
            };

            return View(ticketDetailsVM);
        }

        public IActionResult Edit(int projectId, int ticketId)
        {
            Project project = _projectRepo.Find(projectId);
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Reporter,Developer,Reviewer,Priority,Status,Type");

            IEnumerable<SelectListItem> types = _ticketRepo.GetAllTypes();
            IEnumerable<SelectListItem> priorities = _ticketRepo.GetAllPriorities();

            EditTicketVM editTicketVM = new EditTicketVM
            {
                Project = project,
                Ticket = ticket,
                Priorities = priorities,
                Types = types
            };

            return View(editTicketVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public IActionResult EditPost(int id, string title, string description, string createdAt, string closedAt,
            int projectId, string reporterId, string developerId, string reviewerId, int statusId, int priorityId, int typeId)
        {
            string message = null;
            string messageType = WC.MessageTypeSuccess;

            DateTime createdDate = DateTime.Parse(createdAt);
            DateTime? closedDate = closedAt != null ? DateTime.Parse(closedAt) : null;
            Ticket ticket = new Ticket
            {
                Id = id,
                Title = title ?? "-",
                Description = description,
                CreatedAt = createdDate,
                ClosedAt = closedDate,
                ProjectId = projectId,
                ReporterId = reporterId,
                DeveloperId = developerId,
                ReviewerId = reviewerId,
                StatusId = statusId,
                PriorityId = priorityId,
                TypeId = typeId,
            };

            try
            {
                _ticketRepo.Update(ticket);
                _ticketRepo.Save();

                message = "Ticket updated successfully";
                messageType = WC.MessageTypeSuccess;
            }
            catch (Exception)
            {
                // Error
                message = "Something went wrong";
                messageType = WC.MessageTypeGeneralError;
            }

            return Json(new { redirectToUrl = Url.Action(nameof(Index), new { id = ticket.ProjectId, messageType, message }) });
        }

        [HttpGet]
        public IActionResult ChangeStatus(int id)
        {
            IEnumerable<SelectListItem> statuses = _ticketRepo.GetAllStatuses();
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == id, includeProperties: "Status");

            ChangeTicketStatusVM changeTicketStatusVM = new ChangeTicketStatusVM()
            {
                Statuses = statuses,
                Status = ticket.Status,
                Ticket = ticket
            };

            return PartialView("_ChageTicketStatusModal", changeTicketStatusVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ChangeStatus")]
        public IActionResult ChangeStatusPost(TicketStatus status, int ticketId)
        {
            Ticket ticket = _ticketRepo.Find(ticketId);

            ticket.StatusId = status.Id;

            try
            {
                _ticketRepo.Update(ticket);
                _ticketRepo.Save();

                string message = "Ticket status updated successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                // Error
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);

            }

            return RedirectToAction(nameof(Edit), new { projectId = ticket.ProjectId, ticketId = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteTicket")]
        public IActionResult DeleteTicket(int ticketId, int projectId)
        {
            try
            {
                Ticket ticket = _ticketRepo.Find(ticketId);
                _ticketRepo.Remove(ticket);
                _ticketRepo.Save();

                string message = "Ticket deleted successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                // Error
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), new { id = projectId });
        }

        [HttpPost]
        public IActionResult UploadFile(int? projectId = 0, int? ticketId = 0, string path = null)
        {
            if (projectId <= 0)
                return RedirectToAction(nameof(Index), "Home");

            if (ticketId <= 0)
                return RedirectToAction(nameof(Index), new { id = projectId });

            // Empty path
            if (string.IsNullOrEmpty(path))
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
                return RedirectToAction(nameof(Edit), new { projectId, ticketId });
            }

            string webRootPath = _web.WebRootPath;

            // Upload file
            string uploadFolder = webRootPath + WC.AttachmentsPath;
            string fileName = path.Split('\\').LastOrDefault();
            string destFile = uploadFolder + fileName;
            var files = HttpContext.Request.Form.Files;

            //using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            //{
            //    System.IO.File.OpenRead(path).CopyTo(fileStream);
            //}


            return RedirectToAction(nameof(Edit), new { projectId, ticketId });
        }
    }
}
