using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRReport.Data;
using HRReport.Models;
using MiniExcelLibs;

namespace HRReport.Controllers
{
    public class SalesLaborCostController : Controller
    {
        private readonly HRRDbContext _context;

        public SalesLaborCostController(HRRDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị danh sách dữ liệu
        public async Task<IActionResult> Index()
        {
            // Nối bảng Restaurant để lấy tên nhà hàng hiện lên cho đẹp
            var data = await _context.SalesLaborCosts
                .Include(s => s.Restaurant)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
            return View(data);
        }

        // 2. Xử lý Import Excel
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length <= 0) 
            {
                TempData["Error"] = "Mày chưa chọn file mà mạy!";
                return RedirectToAction(nameof(Index));
            }

            // Dùng MemoryStream để MiniExcel đọc trực tiếp, khỏi qua file trung gian
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                
                // Đọc dữ liệu từ Excel và ép kiểu về SalesLaborCost
                var rows = stream.Query<SalesLaborCost>().ToList();

                if (rows.Any())
                {
                    foreach (var row in rows)
                    {
                        // Bước này quan trọng: Kiểm tra xem mã nhà hàng có trong danh mục chưa
                        var storeExists = await _context.Restaurants.AnyAsync(r => r.ResCode == row.ResCode);
                        
                        if (storeExists)
                        {
                            _context.SalesLaborCosts.Add(row);
                        }
                        // Nếu không tồn tại ResCode, dòng này sẽ bị bỏ qua để tránh lỗi khóa ngoại
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã nạp thành công {rows.Count} dòng dữ liệu Sales!";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}