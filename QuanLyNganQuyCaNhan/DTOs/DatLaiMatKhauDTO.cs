using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.DTOs
{
    public class DatLaiMatKhauDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã khôi phục không được để trống")]
        [StringLength(100)]
        public string MaKhoiPhuc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [MaxLength(72)]
        public string MatKhauMoi { get; set; } = string.Empty;

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
        public string? MaOTP { get; set; }
    }
}
