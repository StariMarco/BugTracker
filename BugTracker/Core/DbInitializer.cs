using System;
using System.Linq;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Core
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                // Resulve pending migrations
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {

            }

            // Init roles
            if (!_roleManager.RoleExistsAsync(WC.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(WC.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WC.UserRole)).GetAwaiter().GetResult();
            }
            else
            {
                return;
            }

            InitAdminUser();
        }

        private void InitAdminUser()
        {
            const string adminEmail = "admin@gmail.com";
            _userManager.CreateAsync(new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Admin"
            }, "Admin123*").GetAwaiter().GetResult();

            AppUser user = _db.AppUsers.FirstOrDefault(u => u.Email == adminEmail);
            _userManager.AddToRoleAsync(user, WC.AdminRole).GetAwaiter().GetResult();
        }

        private void InitTables()
        {
            // Init project roles
            _db.ProjectRoles.Add(new ProjectRole { Name = "Project Manager" });
            _db.ProjectRoles.Add(new ProjectRole { Name = "Reporter" });
            _db.ProjectRoles.Add(new ProjectRole { Name = "Developer" });
            _db.ProjectRoles.Add(new ProjectRole { Name = "Reviewer" });

            // Init action types
            _db.ActionTypes.Add(new ActionType { Text = "created the", Type = "Ticket" });
            _db.ActionTypes.Add(new ActionType { Text = "updated the", Type = "Title" });
            _db.ActionTypes.Add(new ActionType { Text = "updated the", Type = "Description" });
            _db.ActionTypes.Add(new ActionType { Text = "changed the", Type = "Type" });
            _db.ActionTypes.Add(new ActionType { Text = "changed the", Type = "Priority" });
            _db.ActionTypes.Add(new ActionType { Text = "changed the", Type = "Status" });
            _db.ActionTypes.Add(new ActionType { Text = "changed the", Type = "Developer" });
            _db.ActionTypes.Add(new ActionType { Text = "added an", Type = "Attachment" });
            _db.ActionTypes.Add(new ActionType { Text = "edited a", Type = "Comment" });
            _db.ActionTypes.Add(new ActionType { Text = "deleted an", Type = "Attachment" });
            _db.ActionTypes.Add(new ActionType { Text = "deleted a", Type = "Comment" });

            // Init ticket priotities
            _db.TicketPriorities.Add(new TicketPriority { Name = "Low", OrderValue = 1 });
            _db.TicketPriorities.Add(new TicketPriority { Name = "Medium", OrderValue = 2 });
            _db.TicketPriorities.Add(new TicketPriority { Name = "High", OrderValue = 3 });

            // Init ticket statuses
            _db.TicketStatus.Add(new TicketStatus { Name = "To Do" });
            _db.TicketStatus.Add(new TicketStatus { Name = "In Progress" });
            _db.TicketStatus.Add(new TicketStatus { Name = "In Review" });
            _db.TicketStatus.Add(new TicketStatus { Name = "Done" });

            // Init ticket types
            _db.TicketTypes.Add(new TicketType { Name = "Bug" });
            _db.TicketTypes.Add(new TicketType { Name = "Improvement" });
            _db.TicketTypes.Add(new TicketType { Name = "New Feature" });

            _db.SaveChanges();
        }
    }
}
