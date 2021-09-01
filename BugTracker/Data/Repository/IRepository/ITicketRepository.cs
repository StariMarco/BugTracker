using System;
using BugTracker.Models;

namespace BugTracker.Data.Repository.IRepository
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        void Update(Ticket obj);
    }
}
