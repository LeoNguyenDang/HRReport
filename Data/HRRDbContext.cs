using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HRReport.Models; // Đảm bảo có dòng này để nó thấy file Restaurant và SalesLaborCost

namespace HRReport.Data
{
    // Kế thừa IdentityDbContext để dùng chung với bảng User của Microsoft
    public class HRRDbContext : IdentityDbContext<AppUser>
    {
        public HRRDbContext(DbContextOptions<HRRDbContext> options)
            : base(options)
        {
        }

        // 1. Khai báo các bảng dữ liệu của mày vào đây
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<SalesLaborCost> SalesLaborCosts { get; set; }

        // 2. Nơi thiết lập ma thuật: Nối bảng và Tính toán tự động
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Phải giữ lại dòng này để các bảng User của Microsoft không bị lỗi
            base.OnModelCreating(modelBuilder);

            // Cấu hình cho bảng Restaurant (Bảng cha)
            modelBuilder.Entity<Restaurant>(entity =>
            {
                // Ép ResCode phải là duy nhất để dùng làm khóa nối
                entity.HasIndex(r => r.ResCode).IsUnique();
            });

            // Cấu hình cho bảng SalesLaborCost (Bảng con)
            modelBuilder.Entity<SalesLaborCost>(entity =>
            {
                // THIẾT LẬP NỐI BẢNG: Dùng ResCode (Chuỗi) thay vì Id (Số)
                entity.HasOne(s => s.Restaurant)
                      .WithMany(r => r.SalesLaborCosts)
                      .HasPrincipalKey(r => r.ResCode)
                      .HasForeignKey(s => s.ResCode);

                // TỰ ĐỘNG TÍNH: LaborCost = Lương MG + Lương TM + Phúc lợi
                entity.Property(e => e.LaborCost)
                      .HasComputedColumnSql("[MgSalary] + [TmSalary] + [Benefit]");

                // TỰ ĐỘNG TÍNH: PercentCol (Dùng NULLIF để tránh lỗi chia cho 0)
                entity.Property(e => e.PercentCol)
                      .HasComputedColumnSql("(([MgSalary] + [TmSalary] + [Benefit]) * 100.0) / NULLIF([Sales], 0)");
            });
        }
    }
}