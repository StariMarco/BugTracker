using System;
using System.Collections.Generic;

namespace BugTracker
{
    public static class WC
    {
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
        public const string MessageTypeGeneralError = "general-error";

        public static Dictionary<int, string> StatusClassMap = new Dictionary<int, string>
        {
             {1, "todo-tag" },
             {2, "in-progress-tag" },
             {3, "in-review-tag" },
             {4, "done-tag" },
        };
    }
}
