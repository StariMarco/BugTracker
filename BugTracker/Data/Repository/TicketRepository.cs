using System;
using BugTracker.Data.Repository.IRepository;
using BugTracker.Models;

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
    }
}
