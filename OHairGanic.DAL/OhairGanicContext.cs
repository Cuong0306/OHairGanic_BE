using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace OHairGanic.DAL;

public partial class OhairGanicContext : DbContext
{
    private readonly IConfiguration _configuration;
    public OhairGanicContext()
    {
    }

    public OhairGanicContext(DbContextOptions<OhairGanicContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<DataPrivacyLog> DataPrivacyLogs { get; set; }

    public virtual DbSet<HairLog> HairLogs { get; set; }

    public virtual DbSet<HairRoutine> HairRoutines { get; set; }

    public virtual DbSet<HairScan> HairScans { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Uid=sa;Pwd=12345;Database=OHairGanic;TrustServerCertificate=True");*/
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection"); // DefaultConnection is defined in appsettings.json
            optionsBuilder.UseSqlServer(connectionString);

        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brand__DAD4F3BE97D31188");

            entity.ToTable("Brand");

            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.Contact).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Website).HasMaxLength(200);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__51BCD7975712A83E");

            entity.ToTable("Cart");

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Cart__UserID__5DCAEF64");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2AA7B7AF2B");

            entity.ToTable("CartItem");

            entity.Property(e => e.CartItemId).HasColumnName("CartItemID");
            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItem__CartID__628FA481");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__CartItem__Produc__6383C8BA");
        });

        modelBuilder.Entity<DataPrivacyLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__DataPriv__5E5499A8177FC754");

            entity.ToTable("DataPrivacyLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(200);
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasColumnName("IP");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.DataPrivacyLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DataPriva__UserI__7F2BE32F");
        });

        modelBuilder.Entity<HairLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__HairLog__5E5499A8BF7CE2EB");

            entity.ToTable("HairLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.LogDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.HairLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__HairLog__UserID__76969D2E");
        });

        modelBuilder.Entity<HairRoutine>(entity =>
        {
            entity.HasKey(e => e.RoutineId).HasName("PK__HairRout__A6E3E51A6D6D296C");

            entity.ToTable("HairRoutine");

            entity.Property(e => e.RoutineId).HasColumnName("RoutineID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.HairRoutines)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__HairRouti__UserI__72C60C4A");
        });

        modelBuilder.Entity<HairScan>(entity =>
        {
            entity.HasKey(e => e.ScanId).HasName("PK__HairScan__63B32661A7A7BA93");

            entity.ToTable("HairScan");

            entity.Property(e => e.ScanId).HasColumnName("ScanID");
            entity.Property(e => e.Dryness).HasMaxLength(50);
            entity.Property(e => e.HairLoss).HasMaxLength(50);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(250)
                .HasColumnName("ImageURL");
            entity.Property(e => e.OilLevel).HasMaxLength(50);
            entity.Property(e => e.ScanDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Tangle).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.HairScans)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__HairScan__UserID__6EF57B66");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAFEFADB2C7");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Order__UserID__66603565");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A11EA43D0C");

            entity.ToTable("OrderItem");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", true)
                .HasColumnType("decimal(21, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__6B24EA82");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderItem__Produ__6C190EBB");
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.PartnerId).HasName("PK__Partner__39FD633275E1A132");

            entity.ToTable("Partner");

            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.Contact).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__B40CC6ED86B13C12");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(250)
                .HasColumnName("ImageURL");
            entity.Property(e => e.Ingredients).HasMaxLength(500);
            entity.Property(e => e.IsOrganic).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.Stock).HasDefaultValue(0);
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Product__BrandID__5812160E");

            entity.HasOne(d => d.Partner).WithMany(p => p.Products)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__Product__Partner__59063A47");

            entity.HasOne(d => d.Shop).WithMany(p => p.Products)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK__Product__ShopID__571DF1D5");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PK__Shop__67C55629E379B699");

            entity.ToTable("Shop");

            entity.Property(e => e.ShopId).HasColumnName("ShopID");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Contact).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(250)
                .HasColumnName("LogoURL");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.OwnerUserId).HasColumnName("OwnerUserID");

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.Shops)
                .HasForeignKey(d => d.OwnerUserId)
                .HasConstraintName("FK__Shop__OwnerUserI__4E88ABD4");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Subscrip__9A2B24BD7EC775CC");

            entity.ToTable("Subscription");

            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(e => e.AutoRenew).HasDefaultValue(true);
            entity.Property(e => e.Division).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Subscript__UserI__7A672E12");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC10C612ED");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534F2D97385").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.HairType).HasMaxLength(50);
            entity.Property(e => e.Lifestyle).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Customer");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
