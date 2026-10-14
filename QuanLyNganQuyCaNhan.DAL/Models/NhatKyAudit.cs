namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class NhatKyAudit
    {
        public long MaAudit { get; set; }
        public DateTime ThoiGian { get; set; }
        public int? MaNguoiDung { get; set; }

        public string HanhDong { get; set; } = string.Empty;

        public string TenBang { get; set; } = string.Empty;

        public string? MaBanGhi { get; set; }

        public string? DuLieuCu { get; set; }

        public string? DuLieuMoi { get; set; }

        public string? DiaChiIP { get; set; }
    }

    public class BoLocNhatKyAudit
    {
        public int? MaNguoiDung { get; set; }
        public string? TenBang { get; set; }
        public string? HanhDong { get; set; }
        public string? MaBanGhi { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public int Trang { get; set; } = 1;
        public int KichThuocTrang { get; set; } = 20;
    }
}
