using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRReport.Models;

namespace HRReport.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào trang quản lý này
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Xem danh sách User
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }
            return View(userRolesViewModel);
        }

        // 2. Trang tạo User mới
        public IActionResult Create()
        {
            // Đưa danh sách 5 Role vào Dropdown cho mày chọn
            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string email, string password, string role)
        {
            var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

                // 1. Trang hiển thị Form Edit
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Đưa danh sách Roles vào Dropdown
            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name", userRoles.FirstOrDefault());

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Role = userRoles.FirstOrDefault()
            };
            return View(model);
        }

        // 2. Xử lý lưu dữ liệu sau khi sửa
        [HttpPost]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Cập nhật Email (Nếu muốn)
            user.Email = model.Email;
            user.UserName = model.Email;

            var roles = await _userManager.GetRolesAsync(user);
            // Xóa hết Role cũ
            await _userManager.RemoveFromRolesAsync(user, roles);
            // Thêm Role mới đã chọn
            await _userManager.AddToRoleAsync(user, model.Role);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 1. Trang xác nhận xóa (Để hỏi lại cho chắc)
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // 2. Xử lý xóa thật sau khi nhấn xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Kiểm tra nếu là tài khoản đang đăng nhập thì không cho tự xóa chính mình
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.Id == user.Id)
                {
                    TempData["Error"] = "Mày không được tự xóa chính mình nha mạy!";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

    }

    // Cái khuôn để hiển thị dữ liệu ra View
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}