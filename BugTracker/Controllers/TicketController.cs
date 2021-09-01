using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly INotyfService _notyf;

        public TicketController(IProjectRepository projectRepo, ITicketRepository ticketRepo, INotyfService notyf)
        {
            _projectRepo = projectRepo;
            _ticketRepo = ticketRepo;
            _notyf = notyf;
        }
        public IActionResult Index(int id)
        {
            Project project = _projectRepo.Find(id);
            IEnumerable<Ticket> tickets = _ticketRepo.GetAll(t => t.ProjectId == project.Id);

            TicketIndexVM ticketIndexVM = new TicketIndexVM
            {
                Project = project,
                Tickets = tickets
            };

            return View(ticketIndexVM);
        }
    }
}
