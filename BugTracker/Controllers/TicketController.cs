using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Core;
using BugTracker.Data;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BugTracker.Controllers
{
	[Authorize]
	public class TicketController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IProjectRepository _projectRepo;
		private readonly ITicketRepository _ticketRepo;
		private readonly IUserProjectRepository _userProjectRepo;
		private readonly INotyfService _notyf;
		private readonly IWebHostEnvironment _web;
		private readonly UserManager<AppUser> _userManager;
		public IConfiguration Configuration { get; }

		public TicketController(ApplicationDbContext db, IProjectRepository projectRepo, ITicketRepository ticketRepo,
			IUserProjectRepository userProjectRepo, INotyfService notyf, IWebHostEnvironment web, UserManager<AppUser> userManager,
			IConfiguration configuration)
		{
			_db = db;
			_projectRepo = projectRepo;
			_ticketRepo = ticketRepo;
			_userProjectRepo = userProjectRepo;
			_notyf = notyf;
			_web = web;
			_userManager = userManager;
			Configuration = configuration;
		}

		#region Tickets Index + Create + Delete Actions

		public IActionResult Index(int projectId, string userfilter = null, int? statusfilter = null, int? typefilter = null,
			string messageType = null, string message = null)
		{
			// Manage toasts
			if (messageType != null)
			{
				HelperFunctions.ManageToastMessages(_notyf, messageType, message);
				return RedirectToAction(nameof(Index), new { projectId = projectId, userfilter, statusfilter });
			}

			string userId = _userManager.GetUserId(HttpContext.User);
			bool isAdmin = User.IsInRole(WC.AdminRole);

			UserProject userProject = IdentityHelper.GetUserProject(_userProjectRepo, isAdmin, projectId, userId);

			Project project = userProject.Project;
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
				Types = types,
				// Can edit if admin, project manager or reporter
				CanCreateTicket = isAdmin || userProject.ProjectRoleId == WC.ProjectManagerId || userProject.ProjectRoleId == WC.ReporterId
			};

			return View(ticketIndexVM);
		}

		public IActionResult Create(int id)
		{
			string userId = _userManager.GetUserId(HttpContext.User);
			bool isAdmin = User.IsInRole(WC.AdminRole);

			UserProject userProject = IdentityHelper.GetUserProject(_userProjectRepo, isAdmin, id, userId);

			Project project = userProject.Project;
			IEnumerable<SelectListItem> types = _ticketRepo.GetAllTypes();
			IEnumerable<SelectListItem> priorities = _ticketRepo.GetAllPriorities();

			CreateTicketVM createTicketVM = new CreateTicketVM
			{
				Project = project,
				Ticket = new Ticket(),
				Types = types,
				Priorities = priorities,
				// Can edit if admin, project manager or reporter
				CanCreateTicket = isAdmin || userProject.ProjectRoleId == WC.ProjectManagerId || userProject.ProjectRoleId == WC.ReporterId
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
				bool canCreateTicket = bool.Parse(Request.Form["canCreateTicket"]);
				if (!canCreateTicket) throw new UnauthorizedAccessException();

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
			catch (UnauthorizedAccessException)
			{
				string message = "You do not have the permission to create a ticket";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
			}
			catch (Exception)
			{
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
			}

			return RedirectToAction(nameof(Index), new { projectId = ticket.ProjectId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("DeleteTicket")]
		public IActionResult DeleteTicket(int ticketId, int projectId)
		{
			try
			{
				bool canDeleteTicket = bool.Parse(Request.Form["canDeleteTicket"]);
				if (!canDeleteTicket) throw new UnauthorizedAccessException();

				Ticket ticket = _ticketRepo.Find(ticketId);

				// Delete the ticket
				_ticketRepo.RemoveTicket(Configuration, ticket);

				_ticketRepo.Save();

				string message = "Ticket deleted successfully";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
			}
			catch (UnauthorizedAccessException)
			{
				string message = "You are not allowed to delete a ticket";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
			}
			catch (Exception)
			{
				// Error
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeGeneralError);
			}

			return RedirectToAction(nameof(Index), new { projectId = projectId });
		}

		#endregion

		#region Edit Ticket

		public IActionResult Edit(int projectId, int ticketId)
		{
			string userId = _userManager.GetUserId(HttpContext.User);
			bool isAdmin = User.IsInRole(WC.AdminRole);

			UserProject up = IdentityHelper.GetUserProject(_userProjectRepo, isAdmin, projectId, userId);

			Project project = up.Project;
			Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Reporter,Developer,Reviewer,Priority,Status,Type");

			IEnumerable<SelectListItem> types = _ticketRepo.GetAllTypes();
			IEnumerable<SelectListItem> priorities = _ticketRepo.GetAllPriorities();
			IEnumerable<TicketAttachment> attachments = _ticketRepo.GetAllAttachments(ticketId);
			IEnumerable<TicketComment> comments = _ticketRepo.GetAllComments(ticketId);
			IEnumerable<HistoryChange> changes = _ticketRepo.GetAllChanges(ticketId);

			bool isAdminOrManager = isAdmin || up.ProjectRoleId == WC.ProjectManagerId;

			EditTicketVM editTicketVM = new EditTicketVM
			{
				Project = project,
				Ticket = ticket,
				Priorities = priorities,
				Types = types,
				Attachments = attachments,
				Comments = comments,
				Comment = new TicketComment(),
				HistoryChanges = changes,
				// Can edit if user is Admin, Project Manager or Reporter
				CanEditTicket = isAdminOrManager || up.ProjectRoleId == WC.ReporterId,
				// Can edit if user is Admin or Project Manager
				CanEditDev = isAdminOrManager
			};

			return View(editTicketVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Edit")]
		public IActionResult EditPost(int id, string title, string description, string createdAt, string closedAt,
			int projectId, string reporterId, string developerId, string reviewerId, int statusId, int priorityId,
			int typeId, string currentUserId, bool canEditTicket = false)
		{
			string message = null;
			string messageType = WC.MessageTypeSuccess;

			try
			{
				if (!canEditTicket) throw new UnauthorizedAccessException();
			}
			catch (UnauthorizedAccessException)
			{
				message = "You are not allowed to edit this ticket";
				messageType = WC.MessageTypeError;
				return Json(new { redirectToUrl = Url.Action(nameof(Index), new { projectId = projectId, messageType, message }) });
			}

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

			return Json(new { redirectToUrl = Url.Action(nameof(Index), new { projectId = ticket.ProjectId, messageType, message }) });
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
		public IActionResult ChangeStatus(int ticketId, int projectId)
		{
			string userId = _userManager.GetUserId(HttpContext.User);
			bool isAdmin = User.IsInRole(WC.AdminRole);
			UserProject userProject = IdentityHelper.GetUserProject(_userProjectRepo, isAdmin, projectId, userId);

			IEnumerable<SelectListItem> statuses = _ticketRepo.GetAllAllowedStatuses(userProject.ProjectRoleId, isAdmin);
			Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Status");

			// Check if the user is the dev assigned to the ticket
			if(userProject.ProjectRoleId == WC.DeveloperId && ticket.DeveloperId != userId)
			{
				statuses = Enumerable.Empty<SelectListItem>();
			}

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

			if(status.Id == 0)
			{
				string message = "You are not allowed to change the status of this ticket";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeNeutral, message);
				return RedirectToAction(nameof(Edit), new { projectId = ticket.ProjectId, ticketId = ticketId });
			}

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
				return RedirectToAction(nameof(Index), new { projectId = projectId });

			var files = HttpContext.Request.Form.Files;

			try
			{
				if (files == null || files.Count == 0)
				{
					Console.WriteLine("User did not select any file");
					string m = "An error occurred";
					HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, m);
					return RedirectToAction(nameof(Edit), new { projectId, ticketId });
				}

				// Upload file
				IFormFile file = files[0];
				long size = file.Length;
				string bucketName = WC.S3BucketName + WC.S3AttachmentsFolderName;

				string documentName = HelperFunctions.UploadFile(Configuration, file).GetAwaiter().GetResult();
				string url = $"https://{bucketName}.s3.amazonaws.com/{documentName}";

				if (string.IsNullOrEmpty(documentName))
				{
					throw new Exception();
				}

				// Add a reference to the ticket in the db
				TicketAttachment attachment = new TicketAttachment
				{
					File = file.FileName,
					Url = url,
					DocumentName = documentName,
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
					After = file.FileName,
					Timestamp = DateTime.Now
				};

				_db.TicketAttachments.Add(attachment);
				_db.HistoryChanges.Add(change);
				_db.SaveChanges();

				string message = "Attachment added successfully";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeSuccess, message);
			}
			catch (AmazonS3Exception)
			{
				string message = "The file upload failed";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.ToString());
				Console.WriteLine(e.StackTrace);
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
				HelperFunctions.DeleteFile(Configuration, attachment.DocumentName).GetAwaiter().GetResult();

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
			catch (AmazonS3Exception)
			{
				string message = "The deletion of the file has failed";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
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

			GetObjectResponse response = HelperFunctions.DownloadFile(Configuration, attachment.DocumentName).GetAwaiter().GetResult();

			if (response == null) return NotFound();

			return File(response.ResponseStream, response.Headers.ContentType, attachment.DocumentName);
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
			string userId = _userManager.GetUserId(HttpContext.User);
			bool isAdmin = User.IsInRole(WC.AdminRole);

			UserProject userProject = IdentityHelper.GetUserProject(_userProjectRepo, isAdmin, projectId, userId, includeProperties: null);

			IEnumerable<UserProject> users = _db.UsersProjects.Include(u => u.User).Where(u => u.ProjectId == projectId && u.ProjectRoleId == WC.DeveloperId);

			ChangeTicketDeveloperVM changeDeveloperVM = new ChangeTicketDeveloperVM()
			{
				Users = users,
				SelectedUser = new AppUser(),
				TicketId = ticketId,
				// Can edit if user is Admin or Project Manager
				CanEditDev = isAdmin || userProject.ProjectRoleId == WC.ProjectManagerId
			};

			return PartialView("EditTicketPage/Modals/_ChangeTicketDeveloperModal", changeDeveloperVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("ChangeDeveloper")]
		public IActionResult ChangeDeveloperPost(int ticketId, AppUser selectedUser)
		{
			string userId = _userManager.GetUserId(HttpContext.User);
			Ticket ticket = _ticketRepo.FirstOrDefault(t => t.Id == ticketId, includeProperties: "Developer");

			try
			{
				bool canEditDev = bool.Parse(Request.Form["canEditDev"]);
				if (!canEditDev) throw new UnauthorizedAccessException();
			}
			catch (UnauthorizedAccessException)
			{
				string message = "You are not allowed to change the developer";
				HelperFunctions.ManageToastMessages(_notyf, WC.MessageTypeError, message);
				return RedirectToAction(nameof(Edit), new { ticket.ProjectId, ticketId });
			}

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
