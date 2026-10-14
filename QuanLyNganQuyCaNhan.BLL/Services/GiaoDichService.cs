using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class GiaoDichService : IGiaoDichService
    {
        private readonly GiaoDichRepository giaoDichRepository;

        public GiaoDichService(GiaoDichRepository giaoDichRepository)
        {
            this.giaoDichRepository = giaoDichRepository;
        }

        public async Task<(IEnumerable<GiaoDich> DanhSach, int TongSoBanGhi)> LayDanhSach(
            int maNguoiDung,
            DateTime? tuNgay,
            DateTime? denNgay,
            int? maDanhMuc,
            int trang,
            int kichThuocTrang,
            string sapXepTheo,
            string chieuSapXep)
        {
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
                throw new ArgumentException("Từ ngày không được sau đến ngày");
            if (maDanhMuc.HasValue && maDanhMuc.Value <= 0)
                throw new ArgumentException("Mã danh mục không hợp lệ");
            if (trang < 1)
                throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1");
            if (kichThuocTrang < 1 || kichThuocTrang > 100)
                throw new ArgumentException("Kích thước trang phải từ 1 đến 100");

            var cacTruongSapXep = new[] { "NgayGiaoDich", "SoTien", "TenDanhMuc", "TenVi", "MaGiaoDich" };
            string? truongHopLe = cacTruongSapXep.FirstOrDefault(
                x => x.Equals(sapXepTheo, StringComparison.OrdinalIgnoreCase));
            if (truongHopLe == null)
                throw new ArgumentException("Trường sắp xếp không hợp lệ");

            string chieuHopLe = chieuSapXep.ToUpperInvariant();
            if (chieuHopLe != "ASC" && chieuHopLe != "DESC")
                throw new ArgumentException("Chiều sắp xếp chỉ được là ASC hoặc DESC");

            return await giaoDichRepository.LayDanhSach(
                maNguoiDung, tuNgay, denNgay, maDanhMuc, trang, kichThuocTrang, truongHopLe, chieuHopLe);
        }

        public async Task<GiaoDich?> LayTheoMa(long maGiaoDich, int maNguoiDung)
        {
            return await giaoDichRepository.LayTheoMa(maGiaoDich, maNguoiDung);
        }

        public async Task<long> ThemGiaoDich(GiaoDich giaoDich)
        {
            await KiemTraVaChuanHoaGiaoDich(giaoDich);
            return await giaoDichRepository.ThemGiaoDich(giaoDich);
        }

        public async Task<int> CapNhatGiaoDich(GiaoDich giaoDich)
        {
            if (giaoDich.PhienBan < 1)
                throw new ArgumentException("Phiên bản giao dịch không hợp lệ");

            await KiemTraVaChuanHoaGiaoDich(giaoDich);
            return await giaoDichRepository.CapNhatGiaoDich(giaoDich);
        }

        public async Task<int> XoaMemGiaoDich(long maGiaoDich, int maNguoiDung)
        {
            return await giaoDichRepository.XoaMemGiaoDich(maGiaoDich, maNguoiDung);
        }

        private async Task KiemTraVaChuanHoaGiaoDich(GiaoDich giaoDich)
        {
            KiemTraDuLieuGiaoDich(giaoDich);

            bool nguoiDungTonTai = await giaoDichRepository.KiemTraNguoiDungTonTai(giaoDich.MaNguoiDung);
            if (!nguoiDungTonTai)
                throw new ArgumentException("Người dùng không tồn tại hoặc đã bị khóa");

            bool viVaDanhMucHopLe = await giaoDichRepository.KiemTraViVaDanhMucHopLe(
                giaoDich.MaNguoiDung, giaoDich.MaVi, giaoDich.MaDanhMuc, giaoDich.LoaiGiaoDich);
            if (!viVaDanhMucHopLe)
                throw new ArgumentException("Ví hoặc danh mục không hợp lệ, không hoạt động, hoặc không khớp loại giao dịch");
        }

        public static void KiemTraDuLieuGiaoDich(GiaoDich giaoDich)
        {
            giaoDich.LoaiGiaoDich = giaoDich.LoaiGiaoDich.Trim().ToUpperInvariant();
            giaoDich.NoiDung = string.IsNullOrWhiteSpace(giaoDich.NoiDung)? null: giaoDich.NoiDung.Trim();

            if (giaoDich.LoaiGiaoDich != "THU" && giaoDich.LoaiGiaoDich != "CHI")
                throw new ArgumentException("Loại giao dịch chỉ được là THU hoặc CHI");
            if (giaoDich.MaVi <= 0 || giaoDich.MaDanhMuc <= 0)
                throw new ArgumentException("Mã ví và mã danh mục phải lớn hơn 0");
            if (giaoDich.SoTien <= 0 || giaoDich.SoTien > 9999999999999999.99m)
                throw new ArgumentException("Số tiền phải lớn hơn 0");
            if (giaoDich.SoTien != decimal.Round(giaoDich.SoTien, 2))
                throw new ArgumentException("Số tiền không được có quá 2 chữ số thập phân");
            if (giaoDich.NgayGiaoDich == default)
                throw new ArgumentException("Ngày giao dịch không hợp lệ");
            if (giaoDich.NoiDung?.Length > 255)
                throw new ArgumentException("Nội dung không được vượt quá 255 ký tự");

        }
    }
}
