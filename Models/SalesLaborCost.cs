using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRReport.Models
{
    public class SalesLaborCost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string ResCode { get; set; } = string.Empty;

        [ForeignKey("ResCode")]
        public virtual Restaurant Restaurant { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Sales { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MgSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TmSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Benefit { get; set; }

        // Database tự tính - Tổng chi phí lương
        public decimal LaborCost { get; private set; }

        // Database tự tính - % Chi phí lương trên doanh thu
        public decimal PercentCol { get; private set; } 
    }
}