/*using System;
using System.Collections.Generic;

namespace InternManagement.Models;

public partial class Chatbox
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public int? GroupReceiverId { get; set; }

    public bool IsGroup { get; set; }

    public virtual Interngroup? GroupReceiver { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User? Receiver { get; set; }

    public virtual User Sender { get; set; } = null!;
}
*/