using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class Product
{
    public int ProductId { get; set; }

    public int? ShopId { get; set; }

    public int? BrandId { get; set; }

    public int? PartnerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Ingredients { get; set; }

    public string? Type { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsOrganic { get; set; }

    public int? Stock { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Partner? Partner { get; set; }

    public virtual Shop? Shop { get; set; }
}
