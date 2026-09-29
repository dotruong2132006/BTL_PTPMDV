namespace QuanLyNganQuyCaNhan.API.Admin.DTOs
{
    public class CapNhatNguoiDungDTO
    {
        public string Email { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public bool TrangThai { get; set; }
    }
}