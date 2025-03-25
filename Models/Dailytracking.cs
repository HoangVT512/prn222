using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Dailytracking
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int TaskId { get; set; }

    public string Report { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
