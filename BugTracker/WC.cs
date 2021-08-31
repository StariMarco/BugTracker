using System;
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

        public const string MessageTypeError = "error";
        public const string MessageTypeSuccess = "success";
    }
}
