
using InternManagement.Models;


namespace InternManagement.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public User Student { get; set; }
        public List<Task> PendingTasks { get; set; }
        public List<Dailytracking> RecentTracking { get; set; }
        public List<Notification> RecentNotifications { get; set; }
        public List<Tasksubmit> RecentSubmissions { get; set; }
        public List<Evaluation> RecentEvaluations { get; set; }
    }
}