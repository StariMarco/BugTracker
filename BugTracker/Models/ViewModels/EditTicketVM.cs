using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class EditTicketVM
    {
        public Project Project { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public IEnumerable<TicketAttachment> Attachments { get; set; }
        public IEnumerable<TicketComment> Comments { get; set; }
        public TicketComment Comment { get; set; }
        public IEnumerable<HistoryChange> HistoryChanges { get; set; }
    }
}
