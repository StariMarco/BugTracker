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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IProjectRepository _projectRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly INotyfService _notyf;

        public TicketController(ApplicationDbContext db, IProjectRepository projectRepo, ITicketRepository ticketRepo, INotyfService notyf)
        {
            _db = db;
            _projectRepo = projectRepo;
            _ticketRepo = ticketRepo;
            _notyf = notyf;
        }

        public IActionResult Index(int id, string userfilter = null, int? statusfilter = null, int? typefilter = null)
        {
            Project project = _projectRepo.Find(id);
            IEnumerable<Ticket> tickets = _ticketRepo.GetAll(t => t.ProjectId == project.Id, includeProperties: "Reporter,Developer,Reviewer,Status,Priority,Type", orderBy: (q) => q.OrderByDescending(t => t.CreatedAt));

            IEnumerable<SelectListItem> statuses = _db.TicketStatus.Select((TicketStatus i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            IEnumerable<SelectListItem> types = _db.TicketTypes.Select((TicketType i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

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
            IEnumerable<SelectListItem> types = _db.TicketTypes.Select((TicketType i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            IEnumerable<SelectListItem> priorities = _db.TicketPriorities.Select((TicketPriority i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

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

        public IActionResult Details(int projectId, int ticketId)
        {
            Project project = _projectRepo.Find(projectId);
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Reporter,Developer,Reviewer,Status,Priority,Type");

            TicketDetailsVM editTicketVM = new TicketDetailsVM
            {
                Project = project,
                Ticket = ticket
            };

            return View(editTicketVM);
        }
    }
}
