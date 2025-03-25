using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Evaluation
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int MentorId { get; set; }

    public int StudentId { get; set; }

    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Mentor { get; set; } = null!;

    public virtual User Student { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
