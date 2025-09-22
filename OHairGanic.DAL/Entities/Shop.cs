using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class Shop
{
    public int ShopId { get; set; }

    public int? OwnerUserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public string? Contact { get; set; }

    public string? LogoUrl { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? OwnerUser { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
