using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.API.User.DTOs
{
    public class CapNhatGiaoDichDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Mã ví không hợp lệ")]
        public int MaVi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Mã danh mục không hợp lệ")]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Loại giao dịch không được để trống")]
        [StringLength(10)]
        public string LoaiGiaoDich { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Số tiền phải lớn hơn 0 và nằm trong giới hạn cho phép")]
        public decimal SoTien { get; set; }

        [StringLength(255, ErrorMessage = "Nội dung không được vượt quá 255 ký tự")]
        public string? NoiDung { get; set; }

        public DateTime NgayGiaoDich { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Phiên bản giao dịch không hợp lệ")]
        public int PhienBan { get; set; }
    }
}
