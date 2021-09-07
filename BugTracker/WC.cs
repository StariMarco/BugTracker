using System;
using System.Collections.Generic;
using BugTracker.Models;

namespace BugTracker
{
    public static class WC
    {
        //TODO: '/' => '\'
        public const string AttachmentsPath = @"/assets/attachments/";

        public const string AdminRole = "Admin";
        public const string UserRole = "User";

        public const string ProjectManagerRole = "ProjectManager";
        public const string ReporterRole = "Reporter";
        public const string DeveloperRole = "Developer";
        public const string ReviewerRole = "Reviewer";

        public const int ProjectManagerId = 1;
        public const int ReporterId = 2;
        public const int DeveloperId = 3;
        public const int ReviewerId = 4;

        public const int StatusToDo = 1;
        public const int StatusInProgress = 2;
        public const int StatusInReview = 3;
        public const int StatusDone = 4;

        public const int TypeBug = 1;
        public const int TypeImprovement = 2;
        public const int TypeNewFeature = 3;

        public const string MessageTypeError = "error";
        public const string MessageTypeSuccess = "success";
        public const string MessageTypeNeutral = "neutral";
        public const string MessageTypeGeneralError = "general-error";

        public static Dictionary<int, string> StatusClassMap = new Dictionary<int, string>
        {
             {1, "todo-tag" },
             {2, "in-progress-tag" },
             {3, "in-review-tag" },
             {4, "done-tag" },
        };

        public static Dictionary<int, string> StatusNameMap = new Dictionary<int, string>
        {
             {1, "To Do" },
             {2, "In Progress" },
             {3, "In Review" },
             {4, "Done" },
        };

        public const int actionTypeCreateTicket = 1;
        public const int actionTypeTitle = 2;
        public const int actionTypeDescription = 3;
        public const int actionTypeType = 4;
        public const int actionTypePriority = 5;
        public const int actionTypeStatus = 6;
        public const int actionTypeDeveloper = 7;
        public const int actionTypeAddAttachment = 8;
        public const int actionTypeEditComment = 9;
        public const int actionTypeDeleteAttachment = 10;
        public const int actionTypeDeleteComment = 11;
    }
}
