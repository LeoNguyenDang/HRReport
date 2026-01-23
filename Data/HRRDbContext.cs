using Microsoft.EntityFrameworkCore;

namespace HRReport.Data
{
    public class HRRDbContext : DbContext
    {
        public HRRDbContext(DbContextOptions<HRRDbContext> options)
            : base(options)
        {
        }

        // Sau này mày muốn quản lý cái gì thì mình sẽ thêm DbSet ở đây sau
        // Ví dụ: public DbSet<TargetCuaTao> Targets { get; set; }
    }
}