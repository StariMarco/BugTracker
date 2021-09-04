using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    }
}
