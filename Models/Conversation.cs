using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Conversation
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool IsGroup { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ConversationMember> ConversationMembers { get; set; }
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
