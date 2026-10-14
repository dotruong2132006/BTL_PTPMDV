namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class GiaoDich
    {
        public long MaGiaoDich { get; set; }

        public int MaNguoiDung { get; set; }

        public int MaVi { get; set; }

        public int MaDanhMuc { get; set; }

        public string LoaiGiaoDich { get; set; } = string.Empty;

        public decimal SoTien { get; set; }

        public string? NoiDung { get; set; }

        public DateTime NgayGiaoDich { get; set; }

        public int PhienBan { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public DateTime? NgayXoa { get; set; }

        public string? TenDanhMuc { get; set; }

        public string? TenVi { get; set; }
    }
}
