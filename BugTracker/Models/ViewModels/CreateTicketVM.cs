﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class CreateTicketVM
    {
        public UserProject UserProject { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
        public bool CanCreateTicket { get; set; }
    }
}
