using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string LoginProvider { get; set; } = null!;

    public string? ProviderId { get; set; }

    public string? Pfp { get; set; }

    public bool IsActive { get; set; }

    public int? GroupId { get; set; }

    public virtual ICollection<Dailytracking> Dailytrackings { get; set; } = new List<Dailytracking>();

    public virtual ICollection<Evaluation> EvaluationMentors { get; set; } = new List<Evaluation>();

    public virtual ICollection<Evaluation> EvaluationStudents { get; set; } = new List<Evaluation>();

    public virtual Interngroup Group { get; set; } = null!;

    public virtual ICollection<Interngroup> Interngroups { get; set; } = new List<Interngroup>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Task> TaskMentors { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskStudents { get; set; } = new List<Task>();

    public virtual ICollection<Tasksubmit> Tasksubmits { get; set; } = new List<Tasksubmit>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
