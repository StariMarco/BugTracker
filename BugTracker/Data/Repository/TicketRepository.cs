using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Core;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Data.Repository
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        private readonly ApplicationDbContext _db;

        public TicketRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Ticket obj)
        {
            _db.Tickets.Update(obj);
        }

        public IEnumerable<SelectListItem> GetAllStatuses()
        {
            return _db.TicketStatus.Select((TicketStatus i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public IEnumerable<SelectListItem> GetAllAllowedStatuses(int userRole, bool isAdmin = false)
        {
            IEnumerable<SelectListItem> statuses = _db.TicketStatus.Select((TicketStatus i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            if (isAdmin) return statuses;

            switch (userRole)
            {
                case WC.ProjectManagerId:
                    // Are allowed to do anything
                    break;
                case WC.ReporterId:
                    // Are not allowed to update the status to done
                    statuses = statuses.Where(s => s.Value == WC.StatusToDo.ToString());
                    break;
                case WC.DeveloperId:
                    // Are not allowed to update the status to done
                    statuses = statuses.Where(s => s.Value != WC.StatusDone.ToString());
                    break;
                case WC.ReviewerId:
                    // Are only allowed to update the status from in review to done and viceversa
                    statuses = statuses.Where(s => s.Value == WC.StatusDone.ToString() || s.Value == WC.StatusInReview.ToString());
                    break;
                default:
                    return Enumerable.Empty<SelectListItem>();
            }

            return statuses;
        }

        public IEnumerable<SelectListItem> GetAllTypes()
        {
            return _db.TicketTypes.Select((TicketType i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public IEnumerable<SelectListItem> GetAllPriorities()
        {
            return _db.TicketPriorities.Select((TicketPriority i) => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
        }

        public void RemoveTicket(IConfiguration configuration, Ticket obj)
        {
            int ticketId = obj.Id;

            // Delete the ticket
            _db.Tickets.Remove(obj);

            // Delete all attachments
            var attachments = _db.TicketAttachments.Where(a => a.TicketId == ticketId).ToList();
            attachments.ForEach(a => HelperFunctions.DeleteFile(configuration, a.DocumentName));
            _db.TicketAttachments.RemoveRange(attachments);

            // Delete all comments
            _db.TicketComments.RemoveRange(_db.TicketComments.Where(c => c.TicketId == ticketId));

            // Delete all history
            _db.HistoryChanges.RemoveRange(_db.HistoryChanges.Where(c => c.TicketId == ticketId));
        }

        public IEnumerable<TicketAttachment> GetAllAttachments(int ticketId)
        {
            return _db.TicketAttachments.Include(a => a.User).Where(a => a.TicketId == ticketId);
        }

        public IEnumerable<TicketComment> GetAllComments(int ticketId)
        {
            return _db.TicketComments.OrderByDescending(c => c.CreatedAt).Include(a => a.User).Where(a => a.TicketId == ticketId);
        }

        public IEnumerable<HistoryChange> GetAllChanges(int ticketId)
        {
            return _db.HistoryChanges.OrderByDescending(c => c.Timestamp).Include(a => a.User).Include(a => a.ActionType).Where(a => a.TicketId == ticketId);
        }
    }
}
