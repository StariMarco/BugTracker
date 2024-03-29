﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class TicketIndexVM
    {
        public UserProject UserProject { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public bool CanCreateTicket { get; set; }
    }
}
