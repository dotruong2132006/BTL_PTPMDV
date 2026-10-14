namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class NhacNho
    {
        public int MaNhacNho { get; set; }
        public int MaNguoiDung { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? NoiDung { get; set; }
        public DateTime NgayNhac { get; set; }
        public bool LapLai { get; set; }
        public string? KieuLapLai { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? NgayNhacTiepTheo { get; set; }
        public int PhienBan { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public DateTime? NgayXoa { get; set; }
    }

    public class ThongBaoNhacNho
    {
        public long MaThongBao { get; set; }
        public int MaNhacNho { get; set; }
        public int MaNguoiDung { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? NoiDung { get; set; }
        public DateTime NgayDenHan { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayDoc { get; set; }
        public bool DaDoc => NgayDoc.HasValue;
    }
}
