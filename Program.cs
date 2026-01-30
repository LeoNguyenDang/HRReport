using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HRReport.Data;
using HRReport.Models; // Đã thêm dòng này để thấy AppUser mạy nhé

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database SQLite
builder.Services.AddDbContext<HRRDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Identity (Sử dụng AppUser thay vì IdentityUser)
builder.Services.AddDefaultIdentity<AppUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Không bắt xác thực email
    options.Password.RequireDigit = false; 
    options.Password.RequiredLength = 6; 
    options.Password.RequireNonAlphanumeric = false; 
    options.Password.RequireUppercase = false; 
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>() // Kích hoạt phân quyền
.AddEntityFrameworkStores<HRRDbContext>();

// 3. Cấu hình các dịch vụ cho Web (MVC và Razor Pages)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); 

var app = builder.Build();

// 4. Cấu hình luồng xử lý (Middleware Pipeline)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự Authentication đứng TRƯỚC Authorization
app.UseAuthentication(); 
app.UseAuthorization();

// 5. Định nghĩa các đường dẫn (Routes)
app.MapStaticAssets();
app.MapRazorPages(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 6. TỰ ĐỘNG TẠO USER ADMIN KHI CHẠY APP (SEED DATA)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        // Nhớ là file SeedData.cs cũng phải đổi sang AppUser rồi mới chạy được nha
        await SeedData.Initialize(services);
    }
    catch (Exception ex) 
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi Seed Data rồi mạy ơi!");
    }
} // <--- Kiểm tra xem có thiếu dấu đóng ngoặc này không

app.Run();