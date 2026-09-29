namespace QuanLyNganQuyCaNhan.API.Admin.DTOs
{
    public class NguoiDungDTO
    {
        public int MaNguoiDung { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public bool DaBat2FA { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public DateTime? NgayXoa { get; set; }
    }
}