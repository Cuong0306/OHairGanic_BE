using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? HairType { get; set; }

    public string? Lifestyle { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<DataPrivacyLog> DataPrivacyLogs { get; set; } = new List<DataPrivacyLog>();

    public virtual ICollection<HairLog> HairLogs { get; set; } = new List<HairLog>();

    public virtual ICollection<HairRoutine> HairRoutines { get; set; } = new List<HairRoutine>();

    public virtual ICollection<HairScan> HairScans { get; set; } = new List<HairScan>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
