using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRReport.Data;
using HRReport.Models;
using MiniExcelLibs;
using Microsoft.Data.SqlClient; // Thêm cái này để xài SQL Server Parameter nếu cần

namespace HRReport.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly HRRDbContext _context;

        public RestaurantController(HRRDbContext context)
        {
            _context = context;
        }

        // INDEX - Hiển thị danh sách nhà hàng --------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var data = await _context.Restaurants.ToListAsync();
            return View(data);
        }

        // CREATE - Hiển thị form tạo mới ------------------------------------------------------------
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // EDIT - Hiển thị form chỉnh sửa ------------------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        // EDIT - Xử lý chỉnh sửa (Dành cho SQL Server) ----------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Restaurant restaurant)
        {
            if (id != restaurant.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. Lấy mã cũ từ DB (Dùng AsNoTracking để không bị xung đột bộ nhớ đệm)
                var oldResCode = await _context.Restaurants.AsNoTracking()
                    .Where(r => r.Id == id)
                    .Select(r => r.ResCode)
                    .FirstOrDefaultAsync();

                if (oldResCode == null) return NotFound();

                // Dùng Transaction để đảm bảo: Đổi mã con xong thì mới đổi mã cha
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 2. Nếu người dùng thực sự thay đổi mã ResCode
                        if (oldResCode != restaurant.ResCode)
                        {
                            // BƯỚC A: Cập nhật bảng con (Sales) - SQL Server xử lý cái này rất ngọt
                            await _context.Database.ExecuteSqlInterpolatedAsync(
                                $"UPDATE SalesLaborCosts SET ResCode = {restaurant.ResCode} WHERE ResCode = {oldResCode}"
                            );
                        }

                        // BƯỚC B: Cập nhật thông tin nhà hàng ở bảng chính
                        _context.Update(restaurant);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        TempData["Success"] = "Đã cập nhật và đồng bộ mã trên SQL Server thành công mạy nhé!";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Lỗi khi lưu: " + ex.Message;
                        return View(restaurant);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // DELETE - Xử lý xóa nhà hàng ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            // Kiểm tra xem có dữ liệu vướng không trước khi cho xóa
            var hasSales = await _context.SalesLaborCosts.AnyAsync(s => s.ResCode == restaurant.ResCode);

            if (hasSales)
            {
                TempData["Error"] = $"Không thể xóa! Nhà hàng [{restaurant.ResCode}] đang có dữ liệu doanh thu.";
                return RedirectToAction(nameof(Index));
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa nhà hàng sạch sẽ!";
            return RedirectToAction(nameof(Index));
        }

        // CHECK RELATED DATA - API kiểm tra dữ liệu trước khi xóa ------------------------------------
        [HttpGet]
        public async Task<JsonResult> CheckRelatedData(string resCode)
        {
            var salesCount = await _context.SalesLaborCosts.CountAsync(s => s.ResCode == resCode);
            var relatedIssues = new List<object>();
            
            if (salesCount > 0) 
                relatedIssues.Add(new { table = "Doanh số & Công nhật", count = salesCount });

            return Json(new { hasData = relatedIssues.Any(), details = relatedIssues });
        }

        // IMPORT - Xử lý nạp file Excel -------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length <= 0) 
            {
                TempData["Error"] = "Mày chưa chọn file mà mạy!";
                return RedirectToAction(nameof(Index));
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                var rows = stream.Query<Restaurant>().ToList();

                if (rows.Any())
                {
                    foreach (var row in rows)
                    {
                        var existingStore = await _context.Restaurants
                            .FirstOrDefaultAsync(r => r.ResCode == row.ResCode);

                        if (existingStore == null)
                        {
                            _context.Restaurants.Add(row);
                        }
                        else
                        {
                            // Cập nhật thông tin nếu đã tồn tại
                            _context.Entry(existingStore).CurrentValues.SetValues(row);
                        }
                    }
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Nạp danh mục thành công rồi mạy!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}