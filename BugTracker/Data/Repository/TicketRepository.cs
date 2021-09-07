using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Core;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        public void RemoveTicket(IWebHostEnvironment web, Ticket obj)
        {
            int ticketId = obj.Id;

            // Delete the ticket
            _db.Tickets.Remove(obj);

            // Delete all attachments
            var attachments = _db.TicketAttachments.Where(a => a.TicketId == ticketId).ToList();
            attachments.ForEach(a => HelperFunctions.DeleteFile(web, a.File));
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
