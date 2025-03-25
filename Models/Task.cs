using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Task
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Deadline { get; set; }

    public int StatusId { get; set; }

    public int MentorId { get; set; }

    public int? GroupId { get; set; }

    public int? StudentId { get; set; }

    public string? File { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Dailytracking> Dailytrackings { get; set; } = new List<Dailytracking>();

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual Interngroup? Group { get; set; }

    public virtual User Mentor { get; set; } = null!;

    public virtual Taskstatus Status { get; set; } = null!;

    public virtual User? Student { get; set; }

    public virtual ICollection<Tasksubmit> Tasksubmits { get; set; } = new List<Tasksubmit>();
}
