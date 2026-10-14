using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class NhacNhoService : INhacNhoService
    {
        private readonly NhacNhoRepository nhacNhoRepository;
        private readonly ILogger<NhacNhoService> logger;
        public NhacNhoService(NhacNhoRepository nhacNhoRepository, ILogger<NhacNhoService> logger)
        {
            this.nhacNhoRepository = nhacNhoRepository;
            this.logger = logger;
        }

        public async Task<IEnumerable<NhacNho>> LayDanhSach(int maNguoiDung, bool? trangThai)
        {
            var danhSach = (await nhacNhoRepository.LayDanhSach(maNguoiDung, trangThai)).ToList();
            danhSach.ForEach(ChuanHoaGioUTC);
            return danhSach;
        }

        public async Task<NhacNho?> LayTheoMa(int maNhacNho, int maNguoiDung)
        {
            var nhacNho = await nhacNhoRepository.LayTheoMa(maNhacNho, maNguoiDung);
            if (nhacNho != null) ChuanHoaGioUTC(nhacNho);
            return nhacNho;
        }

        public async Task<int> ThemNhacNho(NhacNho nhacNho)
        {
            KiemTraVaChuanHoa(nhacNho);
            int ma = await nhacNhoRepository.ThemNhacNho(nhacNho);
            if (ma == 0) throw new ArgumentException("Người dùng không tồn tại hoặc đã bị khóa");
            return ma;
        }

        public Task<int> CapNhatNhacNho(NhacNho nhacNho)
        {
            KiemTraVaChuanHoa(nhacNho);
            if (nhacNho.PhienBan < 1) throw new ArgumentException("Phiên bản không hợp lệ");
            return nhacNhoRepository.CapNhatNhacNho(nhacNho);
        }

        public Task<int> XoaMemNhacNho(int maNhacNho, int maNguoiDung)
        {
            return nhacNhoRepository.XoaMemNhacNho(maNhacNho, maNguoiDung);
        }

        public async Task XuLyDenHan()
        {
            DateTime hienTai = DateTime.UtcNow;
            var danhSach = await nhacNhoRepository.LayDanhSachDenHan(hienTai);
            foreach (var nhacNho in danhSach)
            {
                (DateTime denHan, DateTime? tiepTheo) lanNhac;
                try { lanNhac = TinhLanNhac(nhacNho, hienTai); }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Lịch nhắc {MaNhacNho} không hợp lệ; cần sửa dữ liệu lịch", nhacNho.MaNhacNho);
                    continue;
                }
                var (denHan, tiepTheo) = lanNhac;
                await nhacNhoRepository.PhatThongBao(nhacNho, denHan, tiepTheo);
            }
        }

        public async Task<(IEnumerable<ThongBaoNhacNho> DanhSach, int TongSoBanGhi)> LayThongBao(
            int maNguoiDung, bool chiChuaDoc, int trang, int kichThuocTrang)
        {
            if (trang < 1 || kichThuocTrang < 1 || kichThuocTrang > 100)
                throw new ArgumentException("Trang phải từ 1; kích thước trang từ 1 đến 100");
            var ketQua = await nhacNhoRepository.LayThongBao(maNguoiDung, chiChuaDoc, trang, kichThuocTrang);
            var danhSach = ketQua.DanhSach.ToList();
            foreach (var thongBao in danhSach)
            {
                thongBao.NgayDenHan = DateTime.SpecifyKind(thongBao.NgayDenHan, DateTimeKind.Utc);
                thongBao.NgayTao = DateTime.SpecifyKind(thongBao.NgayTao, DateTimeKind.Utc);
                if (thongBao.NgayDoc.HasValue) thongBao.NgayDoc = DateTime.SpecifyKind(thongBao.NgayDoc.Value, DateTimeKind.Utc);
            }
            return (danhSach, ketQua.TongSoBanGhi);
        }

        public Task<int> DanhDauDaDoc(long maThongBao, int maNguoiDung)
        {
            return nhacNhoRepository.DanhDauDaDoc(maThongBao, maNguoiDung);
        }

        private static void KiemTraVaChuanHoa(NhacNho nhacNho)
        {
            nhacNho.TieuDe = (nhacNho.TieuDe ?? "").Trim();
            nhacNho.NoiDung = string.IsNullOrWhiteSpace(nhacNho.NoiDung) ? null : nhacNho.NoiDung.Trim();
            nhacNho.KieuLapLai = string.IsNullOrWhiteSpace(nhacNho.KieuLapLai) ? null : nhacNho.KieuLapLai.Trim().ToUpperInvariant();
            if (nhacNho.TieuDe.Length == 0 || nhacNho.TieuDe.Length > 200)
                throw new ArgumentException("Tiêu đề không được để trống và không vượt quá 200 ký tự");
            if (nhacNho.NoiDung?.Length > 500) throw new ArgumentException("Nội dung tối đa 500 ký tự");
            if (nhacNho.NgayNhac == default || nhacNho.NgayNhac.Year > 9998)
                throw new ArgumentException("Ngày nhắc không hợp lệ");
            if (nhacNho.TrangThai && nhacNho.NgayNhac <= DateTime.UtcNow)
                throw new ArgumentException("Ngày nhắc của lịch đang hoạt động phải ở tương lai");
            if (nhacNho.LapLai && !new[] { "HANG_NGAY", "HANG_TUAN", "HANG_THANG" }.Contains(nhacNho.KieuLapLai))
                throw new ArgumentException("Kiểu lặp lại phải là HANG_NGAY, HANG_TUAN hoặc HANG_THANG");
            if (!nhacNho.LapLai) nhacNho.KieuLapLai = null;
            nhacNho.NgayNhacTiepTheo = nhacNho.TrangThai ? nhacNho.NgayNhac : null;
        }

        private static (DateTime DenHan, DateTime? TiepTheo) TinhLanNhac(NhacNho nhacNho, DateTime hienTai)
        {
            if (!nhacNho.LapLai) return (nhacNho.NgayNhacTiepTheo!.Value, null);
            if (!new[] { "HANG_NGAY", "HANG_TUAN", "HANG_THANG" }.Contains(nhacNho.KieuLapLai))
                throw new ArgumentException("Kiểu lặp lại của lịch không được hỗ trợ");
            // Lịch dân sự UTC+7; tính theo mốc gốc để ngày 31 không bị trôi sau tháng ngắn.
            DateTime moc = nhacNho.NgayNhac.AddHours(7);
            DateTime bayGio = hienTai.AddHours(7);
            DateTime denHan;
            int soKy;
            bool hangThang = nhacNho.KieuLapLai == "HANG_THANG";
            int soNgay = nhacNho.KieuLapLai == "HANG_TUAN" ? 7 : 1;
            if (hangThang)
            {
                soKy = (bayGio.Year - moc.Year) * 12 + bayGio.Month - moc.Month;
                if (moc.AddMonths(soKy) > bayGio) soKy--;
                denHan = moc.AddMonths(soKy);
            }
            else
            {
                soKy = (int)Math.Floor((bayGio - moc).TotalDays / soNgay);
                denHan = moc.AddDays((long)soKy * soNgay);
            }
            DateTime? tiepTheo;
            try
            {
                tiepTheo = (hangThang ? moc.AddMonths(soKy + 1) : moc.AddDays(((long)soKy + 1) * soNgay)).AddHours(-7);
            }
            catch (ArgumentOutOfRangeException) { tiepTheo = null; }
            return (denHan.AddHours(-7), tiepTheo);
        }

        private static void ChuanHoaGioUTC(NhacNho nhacNho)
        {
            nhacNho.NgayNhac = DateTime.SpecifyKind(nhacNho.NgayNhac, DateTimeKind.Utc);
            nhacNho.NgayTao = DateTime.SpecifyKind(nhacNho.NgayTao, DateTimeKind.Utc);
            if (nhacNho.NgayNhacTiepTheo.HasValue) nhacNho.NgayNhacTiepTheo = DateTime.SpecifyKind(nhacNho.NgayNhacTiepTheo.Value, DateTimeKind.Utc);
            if (nhacNho.NgayCapNhat.HasValue) nhacNho.NgayCapNhat = DateTime.SpecifyKind(nhacNho.NgayCapNhat.Value, DateTimeKind.Utc);
            if (nhacNho.NgayXoa.HasValue) nhacNho.NgayXoa = DateTime.SpecifyKind(nhacNho.NgayXoa.Value, DateTimeKind.Utc);
        }
    }
}
