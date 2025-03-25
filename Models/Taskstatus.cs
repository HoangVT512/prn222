using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Taskstatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
