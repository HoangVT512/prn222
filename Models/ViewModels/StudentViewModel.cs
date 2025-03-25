namespace InternManagement.Models.ViewModels
{
    public class StudentViewModel
    {
        public User Student { get; set; }
        public Interngroup Group { get; set; }
        public List<TaskProgressSummary> TaskProgress { get; set; }
    }

    public class TaskProgressSummary
    {
        public Task Task { get; set; }
        public Tasksubmit LastSubmission { get; set; }
        public int Progress { get; set; }
        public bool IsOverdue { get; set; }
    }
}