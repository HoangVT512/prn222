namespace InternManagement.Models
{
    public class ConversationMember
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }

        public Conversation Conversation { get; set; }
        public User User { get; set; }
    }
}