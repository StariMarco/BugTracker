﻿using System;
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
        public IActionResult EditPost(Ticket ticket)
        {
            try
            {
                _ticketRepo.Update(ticket);
                _ticketRepo.Save();

                string message = "Ticket updated successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                // Error
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Index), new { id = ticket.ProjectId });
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
        [ValidateAntiForgeryToken]
        public IActionResult UploadFile(int projectId, TicketAttachment attachment)
        {

            return RedirectToAction(nameof(Index), new { id = projectId });
        }
    }
}
