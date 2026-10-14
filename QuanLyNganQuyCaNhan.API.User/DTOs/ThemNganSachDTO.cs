using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.API.User.DTOs
{
    public class ThemNganSachDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Mã danh mục không hợp lệ")]
        public int MaDanhMuc { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Số tiền giới hạn không hợp lệ")]
        public decimal SoTienGioiHan { get; set; }

        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang { get; set; }

        [Range(2000, 9998, ErrorMessage = "Năm phải từ 2000 đến 9998")]
        public int Nam { get; set; }

        [Range(typeof(decimal), "0.01", "100", ErrorMessage = "Phần trăm cảnh báo phải lớn hơn 0 và không vượt quá 100")]
        public decimal PhanTramCanhBao { get; set; } = 80;
    }
}
