using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.DTOs
{
    public class TaoMaKhoiPhucDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [StringLength(128)]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
        public string? MaOTP { get; set; }

        public bool DaHieuVaSeLuuMa { get; set; }
    }
}
