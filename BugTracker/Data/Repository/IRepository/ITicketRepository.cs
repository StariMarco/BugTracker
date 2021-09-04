using System;
using System.Collections.Generic;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Data.Repository.IRepository
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        void Update(Ticket obj);

        public IEnumerable<SelectListItem> GetAllStatuses();
        public IEnumerable<SelectListItem> GetAllTypes();
        public IEnumerable<SelectListItem> GetAllPriorities();
    }
}
