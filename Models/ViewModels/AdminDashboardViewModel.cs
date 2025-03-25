namespace InternManagement.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalMentors { get; set; }
        public int TotalGroups { get; set; }

        public Dictionary<string, int> TasksByStatus { get; set; }
        public List<StudentWithLateSubmission> StudentsWithLateSubmissions { get; set; }
        public List<Notification> RecentNotifications { get; set; }
    }

    public class StudentWithLateSubmission
    {
        public User Student { get; set; }
        public Task Task { get; set; }
        public int DaysLate { get; set; }
    }
}
