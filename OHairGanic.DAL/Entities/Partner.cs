using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class Partner
{
    public int PartnerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? Contact { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
