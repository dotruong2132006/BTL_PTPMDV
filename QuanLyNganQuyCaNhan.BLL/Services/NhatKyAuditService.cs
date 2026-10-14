using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class NhatKyAuditService : INhatKyAuditService
    {
        private readonly NhatKyAuditRepository nhatKyAuditRepository;
        public NhatKyAuditService(NhatKyAuditRepository nhatKyAuditRepository) { this.nhatKyAuditRepository = nhatKyAuditRepository; }

        public Task<(IEnumerable<NhatKyAudit> DanhSach, long TongSoBanGhi)> LayDanhSach(BoLocNhatKyAudit boLoc)
        {
            if (boLoc.Trang < 1 || boLoc.KichThuocTrang < 1 || boLoc.KichThuocTrang > 100)
                throw new ArgumentException("Trang phải từ 1, kích thước trang từ 1–100");
            if (boLoc.MaNguoiDung.HasValue && boLoc.MaNguoiDung <= 0) throw new ArgumentException("Mã người dùng phải lớn hơn 0");
            if (boLoc.TuNgay.HasValue && boLoc.DenNgay.HasValue && boLoc.TuNgay.Value.Date > boLoc.DenNgay.Value.Date)
                throw new ArgumentException("Từ ngày không được sau đến ngày");
            if (boLoc.DenNgay.HasValue && boLoc.DenNgay.Value.Year >= 9999) throw new ArgumentException("Đến ngày phải trước năm 9999");
            if ((boLoc.TuNgay.HasValue && boLoc.TuNgay.Value.Year < 1753) || (boLoc.DenNgay.HasValue && boLoc.DenNgay.Value.Year < 1753))
                throw new ArgumentException("Ngày lọc phải từ năm 1753");
            boLoc.TenBang = ChuanHoa(boLoc.TenBang, 100);
            boLoc.HanhDong = ChuanHoa(boLoc.HanhDong, 50);
            boLoc.MaBanGhi = ChuanHoa(boLoc.MaBanGhi, 100);
            return nhatKyAuditRepository.LayDanhSach(boLoc);
        }

        public Task<NhatKyAudit?> LayTheoMa(long maAudit) => maAudit <= 0
            ? Task.FromResult<NhatKyAudit?>(null) : nhatKyAuditRepository.LayTheoMa(maAudit);

        private static string? ChuanHoa(string? giaTri, int doDai)
        {
            if (string.IsNullOrWhiteSpace(giaTri)) return null;
            giaTri = giaTri.Trim();
            if (giaTri.Length > doDai) throw new ArgumentException($"Bộ lọc không được dài quá {doDai} ký tự");
            return giaTri;
        }
    }
}
