using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Models;

namespace BugTracker.Core
{
    public static class HelperFunctions
    {
        public static IEnumerable<UserProject> FilterUserProjects(IEnumerable<UserProject> items, string userfilter = null, int? rolefilter = null)
        {
            if (!string.IsNullOrEmpty(userfilter))
            {
                // Filter by name and/or email
                items = items.Where(u => u.User.FullName.ToLower().Contains(userfilter.ToLower())
                || u.User.Email.ToLower().Contains(userfilter.ToLower()));
            }

            if (rolefilter != null && rolefilter > 0)
            {
                // Filter by role
                items = items.Where(u => u.ProjectRole.Id == rolefilter);
            }

            return items;
        }

        public static void ManageToastMessages(INotyfService notyf, string messageType = null, string message = null)
        {
            if (messageType != null)
            {
                if (messageType == WC.MessageTypeError)
                {
                    // Show error toast
                    notyf.Error(message);
                    //_notyf.Custom(message, 10, "#E14236", "fas fa-times-circle");
                }
                else if (messageType == WC.MessageTypeSuccess)
                {
                    // Show success toast
                    notyf.Success(message);
                    //_notyf.Custom(message, 10, "#32AA75", "fas fa-check-circle");
                }
            }
        }
    }
}
