using System;
using System.Collections.Generic;

namespace BugTracker.Models.ViewModels
{
    public class ChangeTicketDeveloperVM
    {
        public IEnumerable<UserProject> Users { get; set; }
        public AppUser SelectedUser { get; set; }
        public int TicketId { get; set; }
    }
}
