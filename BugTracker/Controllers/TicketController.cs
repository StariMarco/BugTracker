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
            IEnumerable<Ticket> tickets = _ticketRepo.GetAll(t => t.ProjectId == project.Id, includeProperties: "Reporter,Developer,Reviewer,Status,Priority,Type");

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
    }
}
