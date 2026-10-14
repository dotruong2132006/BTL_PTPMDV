namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class NganSach
    {
        public int MaNganSach { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaDanhMuc { get; set; }
        public decimal SoTienGioiHan { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal PhanTramCanhBao { get; set; } = 80;
        public DateTime NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public DateTime? NgayXoa { get; set; }
    }

    public class TinhTrangNganSach : NganSach
    {
        public string TenDanhMuc { get; set; } = string.Empty;
        public decimal TongChi { get; set; }
        public decimal SoTienConLai { get; set; }
        public decimal PhanTramDaSuDung { get; set; }
        public bool DaChamNguong { get; set; }
        public bool DaVuotNganSach { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? ThongBaoCanhBao { get; set; }
    }
}
