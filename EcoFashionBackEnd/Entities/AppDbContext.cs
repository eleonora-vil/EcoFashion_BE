
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        #region DbSet

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Designer> Designers { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<SavedSupplier> SavedSuppliers { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<DesignsVariant> DesignsVarients { get; set; }
        public DbSet<DesignsMaterial> DesignsMaterials { get; set; }
        public DbSet<DesignImage> DesignImages { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ItemTypeSizeRatio> ItemTypeSizeRatios { get; set; }
        public DbSet<DesignFeature> DesignFeatures { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<DesignerMaterialInventory> DesignerMaterialInventories { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialImage> MaterialImages { get; set; }
        public DbSet<SustainabilityCriteria> SustainabilityCriterias { get; set; }
        public DbSet<MaterialSustainability> MaterialSustainabilities { get; set; }
        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<MaterialTypeBenchmark> MaterialTypesBenchmarks { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogImage> BlogImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderGroup> OrderGroups { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<MaterialStock> MaterialStocks { get; set; }
        public DbSet<MaterialStockTransaction> MaterialStockTransactions { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<ProductInventoryTransaction> ProductInventoryTransactions { get; set; }
        public DbSet<Review> Reviews { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            #region user

            // SUPPLIER PROFILE
            modelBuilder.Entity<Supplier>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // DESIGNER PROFILE
            modelBuilder.Entity<Designer>()
                .HasOne(dp => dp.User)
                .WithMany()
                .HasForeignKey(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // APPLICATION
            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Role)
                .WithMany()
                .HasForeignKey(a => a.TargetRoleId)
                .OnDelete(DeleteBehavior.Restrict);


            // SAVED SUPPLIER
            modelBuilder.Entity<SavedSupplier>()
                .HasOne(ss => ss.Designer)
                .WithMany()
                .HasForeignKey(ss => ss.DesignerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedSupplier>()
                .HasOne(ss => ss.Supplier)
                .WithMany()
                .HasForeignKey(ss => ss.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
             .Property(u => u.Status)
             .HasConversion<string>();

            modelBuilder.Entity<Application>()
                .Property(a => a.Status)
                .HasConversion<string>();
            #endregion
            #region DESIGN
            // ------------------ SIZE & VARIANT ------------------
            // 1 Size -> N Variants | Variant thuộc về 1 Size
            // Restrict: Không cho xoá Size nếu vẫn còn Variant tham chiếu
            modelBuilder.Entity<Size>()
                .HasMany(s => s.Variants)
                .WithOne(v => v.Size)
                .HasForeignKey(v => v.SizeId)
                .OnDelete(DeleteBehavior.Restrict);
            // 1 Design -> N Variants | Variant thuộc về 1 Design
            // Cascade: Xoá Design sẽ xoá luôn Variants liên quan
            modelBuilder.Entity<DesignsVariant>()
                .HasOne(v => v.Design)
                .WithMany(d => d.DesignsVariants)
                .HasForeignKey(v => v.DesignId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------ MATERIAL ------------------
            // Composite Key: 1 cặp (DesignId, MaterialId) là unique
            modelBuilder.Entity<DesignsMaterial>()
                .HasKey(dm => new { dm.DesignId, dm.MaterialId });
            // 1 Design -> N DesignsMaterial | DesignsMaterial thuộc về 1 Design
            // Cascade: Xoá Design sẽ xoá luôn DesignsMaterial liên quan
            modelBuilder.Entity<DesignsMaterial>()
                .HasOne(dm => dm.Designs)
                .WithMany(d => d.DesignsMaterials)
                .HasForeignKey(dm => dm.DesignId)
                .OnDelete(DeleteBehavior.Cascade);
            // 1 Material -> N DesignsMaterial | DesignsMaterial thuộc về 1 Material
            // Restrict: Không cho xoá Material nếu vẫn còn DesignsMaterial tham chiếu
            modelBuilder.Entity<DesignsMaterial>()
                .HasOne(dm => dm.Materials)
                .WithMany()
                .HasForeignKey(dm => dm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------ DESIGN ------------------
            // 1 ItemType -> N Design | Design thuộc về 1 ItemType
            // Restrict: Không cho xoá ItemType nếu vẫn còn Design tham chiếu
            modelBuilder.Entity<Design>()
                .HasOne(d => d.ItemTypes)
                .WithMany(it => it.Designs)
                .HasForeignKey(d => d.ItemTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Design>()
                .Property(d => d.SalePrice)
                .HasPrecision(18, 2);

            // ------------------ ITEM TYPE SIZE RATIO ------------------
            // 1 ItemType -> N ItemTypeSizeRatio | ItemTypeSizeRatio thuộc về 1 ItemType
            // Cascade: Xoá ItemType sẽ xoá luôn ItemTypeSizeRatio liên quan
            modelBuilder.Entity<ItemTypeSizeRatio>()
                .HasOne(x => x.ItemType)
                .WithMany(it => it.TypeSizeRatios)
                .HasForeignKey(x => x.ItemTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            // 1 Size -> N ItemTypeSizeRatio | ItemTypeSizeRatio thuộc về 1 Size
            // Cascade: Xoá Size sẽ xoá luôn ItemTypeSizeRatio liên quan
            modelBuilder.Entity<ItemTypeSizeRatio>()
                .HasOne(x => x.Size)
                .WithMany(sz => sz.TypeSizeRatios)
                .HasForeignKey(x => x.SizeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: 1 cặp (ItemTypeId, SizeId) chỉ có 1 hệ số
            modelBuilder.Entity<ItemTypeSizeRatio>()
                .HasIndex(x => new { x.ItemTypeId, x.SizeId })
                .IsUnique();

            // ------------------ DRAFT PART ------------------
            // 1 Design -> N DraftParts | DraftPart thuộc về 1 Design
            // Cascade: Xoá Design sẽ xoá luôn DraftParts liên quan
            modelBuilder.Entity<DraftPart>()
                .HasOne(dp => dp.Design)
                .WithMany(d => d.DraftParts)
                .HasForeignKey(dp => dp.DesignId)
                .OnDelete(DeleteBehavior.Cascade);
            // 1 Material -> N DraftParts | DraftPart thuộc về 1 Material
            // Restrict: Không cho xoá Material nếu vẫn còn DraftPart tham chiếu
            modelBuilder.Entity<DraftPart>()
                .HasOne(dp => dp.Material)
                .WithMany()
                .HasForeignKey(dp => dp.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<DraftPart>()
            //    .Property(dp => dp.MaterialStatus)
            //    .HasConversion<string>();

            // ------------------ DRAFT SKETCH ------------------
            // 1 Design -> N DraftSketches | DraftSketch thuộc về 1 Design
            // Cascade: Xoá Design sẽ xoá luôn DraftSketches liên quan
            modelBuilder.Entity<DraftSketch>()
                .HasOne(ds => ds.Design)
                .WithMany(d => d.DraftSketches)
                .HasForeignKey(ds => ds.DesignId)
                .OnDelete(DeleteBehavior.Cascade);
            // 1 Image -> N DraftSketches | DraftSketch thuộc về 1 Image
            // Cascade: Xoá Image sẽ xoá luôn DraftSketch liên quan
            modelBuilder.Entity<DraftSketch>()
                .HasOne(ds => ds.Image)
                .WithMany()
                .HasForeignKey(ds => ds.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional: đảm bảo 1 ảnh sketch không bị trùng với cùng 1 design
            modelBuilder.Entity<DraftSketch>()
                .HasIndex(ds => new { ds.DesignId, ds.ImageId })
                .IsUnique();
            modelBuilder.Entity<Design>()
                .Property(d => d.CareInstruction)
                .HasMaxLength(500)
                .IsRequired(false);

            // DesignFeature (trước đây là ProductFeature, đổi tên cho rõ ràng hơn)
            modelBuilder.Entity<DesignFeature>(entity =>
            {
                entity.HasKey(df => df.FeatureId);

                entity.HasOne(df => df.Design)
                    .WithOne(d => d.DesignFeatures)
                    .HasForeignKey<DesignFeature>(df => df.DesignId)
                    .OnDelete(DeleteBehavior.Cascade);  // xóa design sẽ xóa feature luôn
            });
            #endregion


            #region product 
            // ------------------ PRODUCT ------------------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);

                // Design (bắt buộc)
                entity.HasOne(p => p.Design)
                    .WithMany(d => d.Products)
                    .HasForeignKey(p => p.DesignId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Restrict: tránh xóa design làm mất hết sản phẩm

                // Variant (có thể null vì chỉ phục vụ kế hoạch)
                entity.HasOne(p => p.Variant)
                    .WithMany()
                    .HasForeignKey(p => p.VariantId)
                    .OnDelete(DeleteBehavior.SetNull);
                // SetNull: nếu variant kế hoạch bị xóa, product vẫn tồn tại

                // Size (bắt buộc)
                entity.HasOne(p => p.Size)
                    .WithMany()
                    .HasForeignKey(p => p.SizeId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Restrict: tránh xóa size làm mất sản phẩm

                // SKU: unique
                entity.HasIndex(p => p.SKU)
                    .IsUnique();

                // Price precision
                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

            });
            #endregion

            #region Warehouse
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(w => w.WarehouseId);

                // 1 Designer -> N Warehouses
                entity.HasOne(w => w.Designer)
                      .WithMany(d => d.Warehouses)
                      .HasForeignKey(w => w.DesignerId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.Property(w => w.WarehouseType)
                      .IsRequired()
                      .HasMaxLength(50); // "Material" hoặc "Product"
            });
            #endregion
            #region ProductInventory
            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.HasKey(pi => pi.InventoryId);

                // ProductInventory ↔ Product (N-1)
                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Inventories)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Restrict: Không xóa Product nếu còn Inventory (tránh mất dữ liệu tồn kho)

                // ProductInventory ↔ Warehouse (N-1)
                entity.HasOne(pi => pi.Warehouse)
                      .WithMany(w => w.ProductInventories)
                      .HasForeignKey(pi => pi.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Cascade: Xóa Warehouse xóa luôn Inventory trong đó

                entity.Property(pi => pi.QuantityAvailable)
                      .IsRequired();

                entity.Property(pi => pi.LastUpdated)
                      .IsRequired();
            });
            #endregion

            #region ProductInventoryTransaction
            modelBuilder.Entity<ProductInventoryTransaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);

                // Transaction ↔ ProductInventory (N-1)
                entity.HasOne(t => t.ProductInventory)
                      .WithMany(pi => pi.Transactions)
                      .HasForeignKey(t => t.InventoryId)
                      .OnDelete(DeleteBehavior.Cascade);


                // ProductInventoryTransaction ↔ User: Nhiều giao dịch do 1 User thực hiện
                // Transaction ↔ User (N-1)
                entity.HasOne(t => t.User)
                      .WithMany() // Nếu muốn tracking User → Transactions thì tạo ICollection<Transaction> bên User
                      .HasForeignKey(t => t.PerformedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Restrict: Giữ lịch sử, không cho xóa User nếu còn transaction

                entity.Property(t => t.QuantityChanged)
                    .IsRequired();

                entity.Property(t => t.TransactionDate)
                    .IsRequired();

                entity.Property(t => t.TransactionType)
                    .HasMaxLength(50);

                entity.Property(t => t.Notes)
                    .HasMaxLength(500);
            });
            #endregion
            #region Material 
            // -------- INVENTORY (MATERIAL) --------
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(w => w.WarehouseId);
                entity.Property(w => w.Name).HasMaxLength(200);
                entity.Property(w => w.WarehouseType).HasMaxLength(50);
            });

            modelBuilder.Entity<MaterialStock>(entity =>
            {
                entity.HasKey(s => s.StockId);
                entity.Property(s => s.QuantityOnHand).HasPrecision(18, 2);
                entity.Property(s => s.MinThreshold).HasPrecision(18, 2);
                entity.HasOne(s => s.Material)
                    .WithMany()
                    .HasForeignKey(s => s.MaterialId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(s => s.Warehouse)
                    .WithMany()
                    .HasForeignKey(s => s.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(s => new { s.MaterialId, s.WarehouseId }).IsUnique();
            });

            modelBuilder.Entity<MaterialStockTransaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);
                entity.Property(t => t.QuantityChange).HasPrecision(18, 2);
                entity.Property(t => t.BeforeQty).HasPrecision(18, 2);
                entity.Property(t => t.AfterQty).HasPrecision(18, 2);
                entity.Property(t => t.TransactionType).HasMaxLength(50);
                entity.Property(t => t.ReferenceType).HasMaxLength(50);
                entity.HasOne(t => t.Material)
                    .WithMany()
                    .HasForeignKey(t => t.MaterialId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Warehouse)
                    .WithMany()
                    .HasForeignKey(t => t.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.MaterialId, t.WarehouseId, t.CreatedAt });
            });
            // Configure relationships for Materials
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Supplier)
                .WithMany()
                .HasForeignKey(m => m.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Material>()
                .HasOne(m => m.MaterialType)
                .WithMany()
                .HasForeignKey(m => m.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Material>()
                .Property(m => m.PricePerUnit)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Material>()
                .Property(m => m.RecycledPercentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<MaterialImage>()
                .HasOne(mi => mi.Material)
                .WithMany(m => m.MaterialImages)
                .HasForeignKey(mi => mi.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialImage>()
                .HasOne(mi => mi.Image)
                .WithMany()
                .HasForeignKey(mi => mi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialSustainability>()
                .HasKey(ms => new { ms.MaterialId, ms.CriterionId });

            modelBuilder.Entity<MaterialSustainability>()
                .HasOne(ms => ms.Material)
                .WithMany(m => m.MaterialSustainabilityMetrics)
                .HasForeignKey(ms => ms.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaterialSustainability>()
                .HasOne(ms => ms.SustainabilityCriterion)
                .WithMany()
                .HasForeignKey(ms => ms.CriterionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MaterialSustainability>()
                .Property(ms => ms.Value)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DesignerMaterialInventory>()
                .HasOne(dmi => dmi.Designer)
                .WithMany()
                .HasForeignKey(dmi => dmi.DesignerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DesignerMaterialInventory>()
                .HasOne(dmi => dmi.Material)
                .WithMany()
                .HasForeignKey(dmi => dmi.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DesignerMaterialInventory>()
                .Property(dmi => dmi.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaterialTypeBenchmark>()
                .HasOne(mt => mt.MaterialType)
                .WithMany()
                .HasForeignKey(mt => mt.TypeId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MaterialTypeBenchmark>()
                .HasOne(mt => mt.SustainabilityCriteria)
                .WithMany()
                .HasForeignKey(mt => mt.CriteriaId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MaterialTypeBenchmark>()
                .Property(mt => mt.Value)
                .HasPrecision(18, 2);
            #endregion

            #region Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User).WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderGroup)
                .WithMany(g => g.Orders)
                .HasForeignKey(o => o.OrderGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();
            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasConversion<string>();
            modelBuilder.Entity<Order>()
                .Property(o => o.FulfillmentStatus)
                .HasConversion<string>();
            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Discount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.CommissionRate)
                .HasPrecision(5, 4);
            modelBuilder.Entity<Order>()
                .Property(o => o.CommissionAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.NetAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Design).WithMany()
                .HasForeignKey(od => od.DesignId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Designer).WithMany()
                .HasForeignKey(od => od.DesignerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Material).WithMany()
                .HasForeignKey(od => od.MaterialId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Supplier).WithMany()
                .HasForeignKey(od => od.SupplierId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order).WithMany()
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Type)
                .HasConversion<string>();
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Status)
                .HasConversion<string>();
            #endregion Order

            #region PaymentTransaction
            // Order ↔ User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentTransaction ↔ Order
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Order)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentTransaction ↔ User
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.User)
                .WithMany(u => u.PaymentTransactions)
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .HasIndex(pt => pt.TxnRef)
                .IsUnique();

            #endregion

            #region unique
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BlogImage>()
                .HasIndex(bi => new { bi.BlogId, bi.ImageId })
                .IsUnique();
            modelBuilder.Entity<BlogImage>()
                .HasOne(bi => bi.Blog)
                .WithMany(b => b.BlogImages)
                .HasForeignKey(bi => bi.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BlogImage>()
                .HasOne(bi => bi.Image)
                .WithMany()
                .HasForeignKey(bi => bi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Material)
                .WithMany()
                .HasForeignKey(r => r.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .Property(r => r.RatingScore)
                .HasPrecision(2, 1);
            modelBuilder.Entity<Review>()
                .Property(r => r.Comment)
                .HasMaxLength(1000);
            #endregion
            #region Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

        }
    }
}