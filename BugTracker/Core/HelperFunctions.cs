using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AspNetCoreHero.ToastNotification.Abstractions;
using BugTracker.Models;
using Microsoft.AspNetCore.Hosting;

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

        public static IEnumerable<Ticket> FilterTickets(IEnumerable<Ticket> items, string userfilter = null, int? typefilter = null, int? statusfilter = null)
        {
            if (!string.IsNullOrEmpty(userfilter))
            {
                // Filter by name and/or title
                items = items.Where(t => t.Title.ToLower().Contains(userfilter.ToLower())
                || (t.Reporter != null && t.Reporter.FullName.ToLower().Contains(userfilter.ToLower()))
                || (t.Developer != null && t.Developer.FullName.ToLower().Contains(userfilter.ToLower())));
            }

            if (typefilter != null && typefilter > 0)
            {
                // Filter by role
                items = items.Where(u => u.TypeId == typefilter);
            }

            if (statusfilter != null && statusfilter > 0)
            {
                // Filter by role
                items = items.Where(u => u.StatusId == statusfilter);
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
                else if (messageType == WC.MessageTypeGeneralError)
                {
                    // Show general error toast
                    notyf.Error("Something went wrong");
                }
                else if (messageType == WC.MessageTypeNeutral)
                {
                    // Show general error toast
                    notyf.Information(message);
                }
            }
        }

        public static string BytesToString(long value)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (value >= 1024 && order < sizes.Length - 1)
            {
                order++;
                value /= 1024;
            }

            return String.Format("{0:0.##} {1}", value, sizes[order]);
        }

        public static string GetStatusClassFromId(int? id = null, string idString = null)
        {
            int? val;
            try
            {
                val = (id != null) ? id : int.Parse(idString);
            }
            catch (Exception)
            {
                return "todo-tag";
            }

            if (val == null || val == 0) return "todo-tag";

            return WC.StatusClassMap[val ?? 1];
        }

        public static string GetStatusNameFromId(int? id = null, string idString = null)
        {
            int? val;
            try
            {
                val = (id != null) ? id : int.Parse(idString);
            }
            catch (Exception)
            {
                return "To Do";
            }

            if (val == null || val == 0) return "To Do";

            return WC.StatusNameMap[val ?? 1];
        }

        public static void DeleteFile(IWebHostEnvironment web, string filename)
        {
            string webRootPath = web.WebRootPath;
            string uploadFolder = webRootPath + WC.AttachmentsPath;
            string fileToDelete = Path.Combine(uploadFolder, filename);

            if (File.Exists(fileToDelete)) File.Delete(fileToDelete);
        }

        public static string GetAvatarColor(string value)
        {
            string color = "#5343AA";
            int number = 0;

            if (string.IsNullOrEmpty(value)) return color;

            using (MD5 md5 = MD5.Create())
            {
                var hashed = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                number = Math.Abs(BitConverter.ToInt32(hashed, 0)) % WC.AvatarColorMap.Count();
            }

            return WC.AvatarColorMap[number];
        }
    }
}
