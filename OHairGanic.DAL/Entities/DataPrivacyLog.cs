using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class DataPrivacyLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? Ip { get; set; }

    public virtual User? User { get; set; }
}
