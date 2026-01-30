using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HRReport.Models; // Phải có dòng này để nó thấy AppUser

namespace HRReport.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Đổi IdentityUser thành AppUser ở dòng dưới đây
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Danh sách 5 cấp bậc (Giữ nguyên)
            string[] roleNames = { "Admin", "Director", "OM", "AM", "RGM" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo danh sách User mẫu (Tao thêm cột FullName vào đây cho mày luôn)
            // Cấu trúc: Email | Password | Role | FullName
            var usersToSeed = new List<(string Email, string Password, string Role, string FullName)>
            {
                ("admin@hrreport.com", "Admin@123", "Admin", "Hệ Thống Admin"),
                ("director@hrreport.com", "Director@123", "Director", "Giám Đốc Vận Hành"),
                ("om@hrreport.com", "Om@123", "OM", "Quản Lý Vùng (OM)"),
                ("am@hrreport.com", "Am@123", "AM", "Quản Lý Khu Vực (AM)"),
                ("rgm@hrreport.com", "Rgm@123", "RGM", "Quản Lý Nhà Hàng (RGM)")
            };

            foreach (var u in usersToSeed)
            {
                var user = await userManager.FindByEmailAsync(u.Email);
                if (user == null)
                {
                    // Đổi IdentityUser thành AppUser ở đây
                    var newUser = new AppUser 
                    { 
                        UserName = u.Email, 
                        Email = u.Email, 
                        EmailConfirmed = true,
                        FullName = u.FullName // Gán cái tên vào đây nè mạy
                    };
                    
                    var createResult = await userManager.CreateAsync(newUser, u.Password);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, u.Role);
                    }
                }
            }
        }
    }
}