using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class HairScan
{
    public int ScanId { get; set; }

    public int? UserId { get; set; }

    public string? ImageUrl { get; set; }

    public string? OilLevel { get; set; }

    public string? Dryness { get; set; }

    public string? HairLoss { get; set; }

    public string? Tangle { get; set; }

    public DateTime? ScanDate { get; set; }

    public virtual User? User { get; set; }
}
