using Microsoft.AspNetCore.Identity;

namespace HRReport.Models
{
    // Đổi tên thành AppUser như ý mày
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}