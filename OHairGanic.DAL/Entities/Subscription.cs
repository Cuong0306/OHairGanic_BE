using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class Subscription
{
    public int SubscriptionId { get; set; }

    public int? UserId { get; set; }

    public string? Division { get; set; }

    public decimal? Price { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? AutoRenew { get; set; }

    public virtual User? User { get; set; }
}
