using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class HairRoutine
{
    public int RoutineId { get; set; }

    public int? UserId { get; set; }

    public string? RoutineSteps { get; set; }

    public string? RecommendedProducts { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
