namespace InternManagement.Models.ViewModels
{
    public class InternGroupViewModel
    {
        public Interngroup Group { get; set; }
        public List<User> Students { get; set; }
        public List<Task> Tasks { get; set; }
    }

    public class GroupStudentManagementViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string MentorName { get; set; }
        public List<User> StudentsInGroup { get; set; }
        public List<User> AvailableStudents { get; set; }
    }
}