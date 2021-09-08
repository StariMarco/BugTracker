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

        #region Tickets Index + Create + Delete Actions

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

                HistoryChange change = new HistoryChange
                {
                    TicketId = ticket.Id,
                    ProjectId = ticket.ProjectId,
                    UserId = ticket.ReporterId,
                    ActionTypeId = WC.actionTypeCreateTicket,
                    Before = null,
                    After = null,
                    Timestamp = DateTime.Now
                };

                _db.HistoryChanges.Add(change);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteTicket")]
        public IActionResult DeleteTicket(int ticketId, int projectId)
        {
            try
            {
                Ticket ticket = _ticketRepo.Find(ticketId);

                // Delete the ticket
                _ticketRepo.RemoveTicket(_web, ticket);

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

        #endregion

        #region Edit Ticket

        public IActionResult Edit(int projectId, int ticketId)
        {
            Project project = _projectRepo.Find(projectId);
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Reporter,Developer,Reviewer,Priority,Status,Type");

            IEnumerable<SelectListItem> types = _ticketRepo.GetAllTypes();
            IEnumerable<SelectListItem> priorities = _ticketRepo.GetAllPriorities();
            IEnumerable<TicketAttachment> attachments = _ticketRepo.GetAllAttachments(ticketId);
            IEnumerable<TicketComment> comments = _ticketRepo.GetAllComments(ticketId);
            IEnumerable<HistoryChange> changes = _ticketRepo.GetAllChanges(ticketId);

            EditTicketVM editTicketVM = new EditTicketVM
            {
                Project = project,
                Ticket = ticket,
                Priorities = priorities,
                Types = types,
                Attachments = attachments,
                Comments = comments,
                Comment = new TicketComment(),
                HistoryChanges = changes
            };

            return View(editTicketVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public IActionResult EditPost(int id, string title, string description, string createdAt, string closedAt,
            int projectId, string reporterId, string developerId, string reviewerId, int statusId, int priorityId, int typeId, string currentUserId)
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

            ticket.Priority = _db.TicketPriorities.Find(ticket.PriorityId);
            ticket.Type = _db.TicketTypes.Find(ticket.TypeId);

            Ticket oldTicket = _ticketRepo.FirstOrDefault(t => t.Id == id, includeProperties: "Priority,Type", isTracking: false);

            var changes = GetHistoryChanges(ticket, oldTicket, currentUserId);

            try
            {
                _ticketRepo.Update(ticket);
                if (changes.Count > 0) _db.HistoryChanges.AddRange(changes);
                _ticketRepo.Save();

                message = "Ticket updated successfully";
                messageType = WC.MessageTypeSuccess;
            }
            catch (Exception e)
            {
                // Error
                message = "Something went wrong";
                messageType = WC.MessageTypeGeneralError;

                Console.WriteLine(e.Message);
            }

            return Json(new { redirectToUrl = Url.Action(nameof(Index), new { id = ticket.ProjectId, messageType, message }) });
        }

        private List<HistoryChange> GetHistoryChanges(Ticket ticket, Ticket oldTicket, string userId)
        {
            List<HistoryChange> changes = new List<HistoryChange>();

            // Title
            if (ticket.Title?.Trim() != oldTicket.Title?.Trim())
            {
                changes.Add(new HistoryChange
                {
                    TicketId = ticket.Id,
                    ProjectId = ticket.ProjectId,
                    UserId = userId,
                    ActionTypeId = WC.actionTypeTitle,
                    Before = oldTicket.Title,
                    After = ticket.Title,
                    Timestamp = DateTime.Now
                });
            }

            // Description
            if (ticket.Description?.Trim() != oldTicket.Description?.Trim())
            {
                changes.Add(new HistoryChange
                {
                    TicketId = ticket.Id,
                    ProjectId = ticket.ProjectId,
                    UserId = userId,
                    ActionTypeId = WC.actionTypeDescription,
                    Before = string.IsNullOrEmpty(oldTicket.Description) ? "None" : oldTicket.Description,
                    After = string.IsNullOrEmpty(ticket.Description) ? "None" : ticket.Description,
                    Timestamp = DateTime.Now
                });
            }

            // Priority
            if (ticket.PriorityId != oldTicket.PriorityId)
            {
                changes.Add(new HistoryChange
                {
                    TicketId = ticket.Id,
                    ProjectId = ticket.ProjectId,
                    UserId = userId,
                    ActionTypeId = WC.actionTypePriority,
                    Before = oldTicket.Priority.Name,
                    After = ticket.Priority.Name,
                    Timestamp = DateTime.Now
                });
            }

            // Type
            if (ticket.TypeId != oldTicket.TypeId)
            {
                changes.Add(new HistoryChange
                {
                    TicketId = ticket.Id,
                    ProjectId = ticket.ProjectId,
                    UserId = userId,
                    ActionTypeId = WC.actionTypeType,
                    Before = oldTicket.Type.Name,
                    After = ticket.Type.Name,
                    Timestamp = DateTime.Now
                });
            }

            return changes;
        }

        #endregion

        #region Change Ticket Status

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

            return PartialView("EditTicketPage/Modals/_ChageTicketStatusModal", changeTicketStatusVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ChangeStatus")]
        public IActionResult ChangeStatusPost(TicketStatus status, int ticketId, string userId)
        {
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Status");

            if (ticket.StatusId == status.Id)
            {
                // Don't change the status if is equal to the previous one
                string message = "This status is already selected";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeNeutral, message);
                return RedirectToAction(nameof(Edit), new { projectId = ticket.ProjectId, ticketId = ticketId });
            }

            // Get the ticket status before the change
            string statusBefore = ticket.Status.Id.ToString();

            // Change the ticket status
            ticket.StatusId = status.Id;

            // If the new status is "done" => add closedAt
            if (status.Id == WC.StatusDone) ticket.ClosedAt = DateTime.Now;
            else ticket.ClosedAt = null;

            // Create the history record
            HistoryChange change = new HistoryChange
            {
                TicketId = ticket.Id,
                ProjectId = ticket.ProjectId,
                UserId = userId,
                ActionTypeId = WC.actionTypeStatus,
                Before = statusBefore,
                After = status.Id.ToString(),
                Timestamp = DateTime.Now
            };

            try
            {
                // Update
                _ticketRepo.Update(ticket);
                _db.HistoryChanges.Add(change);
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

        #endregion

        #region Attachments

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadFile(int projectId, int ticketId, string userId)
        {
            if (projectId <= 0)
                return RedirectToAction(nameof(Index), "Home");

            if (ticketId <= 0)
                return RedirectToAction(nameof(Index), new { id = projectId });

            string webRootPath = _web.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            try
            {
                if (files == null || files.Count == 0)
                {
                    string m = "An error occurred";
                    HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, m);
                    return RedirectToAction(nameof(Edit), new { projectId, ticketId });
                }

                // Upload file
                string uploadFolder = webRootPath + WC.AttachmentsPath;
                string filename = files[0].FileName;
                long size = files[0].Length;

                using (var fileStream = new FileStream(Path.Combine(uploadFolder, filename), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                // Add a reference to the ticket in the db
                TicketAttachment attachment = new TicketAttachment
                {
                    File = filename,
                    TicketId = ticketId,
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    Size = size
                };

                // Create the ticket history record
                HistoryChange change = new HistoryChange
                {
                    TicketId = ticketId,
                    ProjectId = projectId,
                    UserId = userId,
                    ActionTypeId = WC.actionTypeAddAttachment,
                    Before = "None",
                    After = filename,
                    Timestamp = DateTime.Now
                };

                _db.TicketAttachments.Add(attachment);
                _db.HistoryChanges.Add(change);
                _db.SaveChanges();

                string message = "Attachment added successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }


            return RedirectToAction(nameof(Edit), new { projectId, ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttachment(int projectId, int ticketId, int attachmentId, string userId)
        {
            TicketAttachment attachment = _db.TicketAttachments.Find(attachmentId);
            if (attachment == null) return NotFound();

            try
            {
                // Delete file
                HelperFunctions.DeleteFile(_web, attachment.File);

                // Create the ticket history record
                HistoryChange change = new HistoryChange
                {
                    TicketId = ticketId,
                    ProjectId = projectId,
                    UserId = userId,
                    ActionTypeId = WC.actionTypeDeleteAttachment,
                    Before = attachment.File,
                    After = "None",
                    Timestamp = DateTime.Now
                };

                // Delete attachment record
                _db.TicketAttachments.Remove(attachment);
                _db.HistoryChanges.Add(change);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Edit), new { projectId, ticketId });
        }

        public IActionResult DownloadFile(int attachmentId)
        {
            TicketAttachment attachment = _db.TicketAttachments.Find(attachmentId);
            if (attachment == null) return NotFound();

            string webRootPath = _web.WebRootPath;
            string uploadFolder = webRootPath + WC.AttachmentsPath;
            string fileToDownload = Path.Combine(uploadFolder, attachment.File);

            // Return file
            if (!System.IO.File.Exists(fileToDownload)) return NotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(fileToDownload);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.File);
        }

        #endregion

        #region Comments

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(int projectId, int ticketId, TicketComment comment)
        {
            comment.TicketId = ticketId;
            comment.CreatedAt = DateTime.Now;

            try
            {
                _db.TicketComments.Add(comment);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Edit), new { projectId, ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditComment(int projectId, int ticketId, string oldComment, TicketComment comment)
        {
            comment.TicketId = ticketId;

            if (comment.Text.Trim() == oldComment.Trim())
            {
                return RedirectToAction(nameof(Edit), new { projectId, ticketId });
            }

            // Create the ticket history record
            HistoryChange change = new HistoryChange
            {
                TicketId = ticketId,
                ProjectId = projectId,
                UserId = comment.UserId,
                ActionTypeId = WC.actionTypeEditComment,
                Before = oldComment,
                After = comment.Text,
                Timestamp = DateTime.Now
            };

            try
            {
                _db.TicketComments.Update(comment);
                _db.HistoryChanges.Add(change);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Edit), new { projectId, ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteComment(int projectId, int ticketId, int commentId)
        {
            try
            {
                TicketComment comment = _db.TicketComments.Find(commentId);

                // Create the ticket history record
                HistoryChange change = new HistoryChange
                {
                    TicketId = ticketId,
                    ProjectId = projectId,
                    UserId = comment.UserId,
                    ActionTypeId = WC.actionTypeDeleteComment,
                    Before = comment.Text,
                    After = "None",
                    Timestamp = DateTime.Now
                };

                _db.TicketComments.Remove(comment);
                _db.HistoryChanges.Add(change);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Edit), new { projectId, ticketId });
        }

        #endregion

        #region Change Ticket Developer

        [HttpGet]
        public IActionResult ChangeDeveloper(int projectId, int ticketId)
        {
            IEnumerable<UserProject> users = _db.UsersProjects.Include(u => u.User).Where(u => u.ProjectId == projectId && u.ProjectRoleId == WC.DeveloperId);

            ChangeTicketDeveloperVM changeDeveloperVM = new ChangeTicketDeveloperVM()
            {
                Users = users,
                SelectedUser = new AppUser(),
                TicketId = ticketId
            };

            return PartialView("EditTicketPage/Modals/_ChangeTicketDeveloperModal", changeDeveloperVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ChangeDeveloper")]
        public IActionResult ChangeDeveloperPost(int ticketId, string userId, AppUser selectedUser)
        {
            Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Developer");

            if (ticket.DeveloperId == selectedUser.Id)
            {
                // Don't change the status if is equal to the previous one
                string message = "This developer is already selected";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeNeutral, message);
                return RedirectToAction(nameof(Edit), new { projectId = ticket.ProjectId, ticketId = ticketId });
            }

            // Get the previous dev
            string beforeStr = string.IsNullOrEmpty(ticket.DeveloperId) ? "None" : ticket.Developer.FullName;

            // Update the dev
            ticket.DeveloperId = selectedUser.Id;

            // Create the ticket history record
            HistoryChange change = new HistoryChange
            {
                TicketId = ticket.Id,
                ProjectId = ticket.ProjectId,
                UserId = userId,
                ActionTypeId = WC.actionTypeDeveloper,
                Before = beforeStr,
                After = selectedUser.FullName,
                Timestamp = DateTime.Now
            };

            try
            {
                _ticketRepo.Update(ticket);
                _db.HistoryChanges.Add(change);
                _ticketRepo.Save();

                string message = "Developer update successfully";
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
            }
            catch (Exception)
            {
                HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
            }

            return RedirectToAction(nameof(Edit), new { ticket.ProjectId, ticketId });
        }

        #endregion

    }
}
