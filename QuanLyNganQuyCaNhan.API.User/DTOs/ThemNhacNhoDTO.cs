using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.API.User.DTOs
{
    public class ThemNhacNhoDTO
    {
        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;
        [StringLength(500)]
        public string? NoiDung { get; set; }
        public DateTimeOffset NgayNhac { get; set; }
        public bool LapLai { get; set; }
        [StringLength(50)]
        public string? KieuLapLai { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
