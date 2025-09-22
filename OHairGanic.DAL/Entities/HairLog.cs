using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class HairLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Note { get; set; }

    public DateTime? LogDate { get; set; }

    public virtual User? User { get; set; }
}
