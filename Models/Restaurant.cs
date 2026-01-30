using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRReport.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ResCode { get; set; } = string.Empty; // Mã nhà hàng (R001...)

        public string FinCode { get; set; } = string.Empty; // Mã tài chính
        public string AccCode { get; set; } = string.Empty; // Mã kế toán
        
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty; // Vùng
        
        // Phân cấp quản lý (Cái này cực quan trọng để làm báo cáo nhóm)
        public string AM { get; set; } = string.Empty; // Area Manager
        public string OM { get; set; } = string.Empty; // Operation Manager
        public string OD { get; set; } = string.Empty; // Operation Director
        
        public string Province { get; set; } = string.Empty; // Tỉnh thành
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        public DateTime? OpenDate { get; set; } // Ngày khai trương (Để dấu ? đề phòng chưa có ngày)
        
        public string Status { get; set; } = string.Empty; // "Active" hoặc "Closed"

        // Đường dây kết nối tới bảng SalesLaborCost
        public virtual ICollection<SalesLaborCost> SalesLaborCosts { get; set; }
    }
}