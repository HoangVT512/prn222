using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Tasksubmit
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int StudentId { get; set; }

    public string? SubmittedFile { get; set; }

    public string? Status { get; set; }

    public DateTime SubmittedAt { get; set; }

    public string? Remarks { get; set; }

    public int? ProgressEvaluate { get; set; }

    public string? MentorNote { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
