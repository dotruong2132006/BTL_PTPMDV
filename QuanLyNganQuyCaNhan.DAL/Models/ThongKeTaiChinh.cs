namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class ThongKeThang
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal ChenhLech => TongThu - TongChi;
        public long SoGiaoDich { get; set; }
    }

    public class TongQuanTaiChinh : ThongKeThang
    {
        public decimal TongSoDuVi => DanhSachVi.Sum(x => x.SoDu);
        public List<Vi> DanhSachVi { get; set; } = new();
    }

    public class ThongKeDanhMuc
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string LoaiGiaoDich { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public long SoGiaoDich { get; set; }
        public decimal TyLePhanTram { get; set; }
    }
}
