using System;
using System.Collections.Generic;

namespace BugTracker.Models.ViewModels
{
    public class TicketIndexVM
    {
        public Project Project { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
    }
}
