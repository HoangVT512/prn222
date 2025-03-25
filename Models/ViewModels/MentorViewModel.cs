
using InternManagement.Models;

namespace InternManagement.Models.ViewModels
{
    public class MentorViewModel
    {
        public User Mentor { get; set; }
        public List<Interngroup> Groups { get; set; }
        public List<User> Students { get; set; }
    }

    public class MentorGroupAssignmentViewModel
    {
        public int MentorId { get; set; }
        public string MentorName { get; set; }
        public List<Interngroup> AssignedGroups { get; set; }
        public List<Interngroup> AvailableGroups { get; set; }
    }
}