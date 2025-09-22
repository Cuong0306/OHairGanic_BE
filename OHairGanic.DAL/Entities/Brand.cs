using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class Brand
{
    public int BrandId { get; set; }

    public string Name { get; set; } = null!;

    public string? Country { get; set; }

    public string? Website { get; set; }

    public string? Contact { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
