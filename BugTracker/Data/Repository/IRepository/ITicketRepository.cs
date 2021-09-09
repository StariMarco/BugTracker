using System;
using System.Collections.Generic;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Data.Repository.IRepository
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        void Update(Ticket obj);

        void RemoveTicket(IWebHostEnvironment web, Ticket obj);

        public IEnumerable<SelectListItem> GetAllStatuses();
        public IEnumerable<SelectListItem> GetAllAllowedStatuses(int userRole, bool isAdmin = false);
        public IEnumerable<SelectListItem> GetAllTypes();
        public IEnumerable<SelectListItem> GetAllPriorities();
        public IEnumerable<TicketAttachment> GetAllAttachments(int ticketId);
        public IEnumerable<TicketComment> GetAllComments(int ticketId);
        public IEnumerable<HistoryChange> GetAllChanges(int ticketId);
    }
}
