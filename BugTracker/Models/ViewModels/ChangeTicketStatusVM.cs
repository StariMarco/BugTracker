using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class ChangeTicketStatusVM
    {
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public TicketStatus Status { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<TicketAttachment> Attachments { get; set; }
    }
}
