using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Interngroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Field { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int? MentorId { get; set; }

    public virtual User? Mentor { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
