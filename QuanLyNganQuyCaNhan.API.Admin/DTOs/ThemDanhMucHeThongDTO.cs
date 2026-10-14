using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.API.Admin.DTOs
{
    public class ThemDanhMucHeThongDTO
    {
        [Required, StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;
        [Required, RegularExpression("^(THU|CHI)$")]
        public string LoaiDanhMuc { get; set; } = string.Empty;
        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}
