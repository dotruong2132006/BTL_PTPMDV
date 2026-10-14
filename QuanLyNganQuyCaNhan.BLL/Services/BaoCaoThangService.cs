using Microsoft.Extensions.Logging;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class BaoCaoThangService : IBaoCaoThangService
    {
        private readonly BaoCaoThangRepository baoCaoThangRepository;
        private readonly BaoCaoPdfService baoCaoPdfService;
        private readonly ILogger<BaoCaoThangService> logger;

        public BaoCaoThangService(BaoCaoThangRepository baoCaoThangRepository,
            BaoCaoPdfService baoCaoPdfService, ILogger<BaoCaoThangService> logger)
        {
            this.baoCaoThangRepository = baoCaoThangRepository;
            this.baoCaoPdfService = baoCaoPdfService;
            this.logger = logger;
        }

        public async Task<BaoCaoThang> TaoYeuCau(int maNguoiDung, int thang, int nam, Guid maYeuCau, string? diaChiIP)
        {
            if (thang < 1 || thang > 12 || nam < 2000 || nam > 9998)
                throw new ArgumentException("Tháng từ 1 đến 12, năm từ 2000 đến 9998");
            if (maYeuCau == Guid.Empty) throw new ArgumentException("Mã yêu cầu phải là GUID khác rỗng");
            var baoCao = await baoCaoThangRepository.TaoYeuCau(maNguoiDung, thang, nam, maYeuCau, diaChiIP);
            ChuanHoaGioUTC(baoCao);
            return baoCao;
        }

        public async Task<BaoCaoThang?> LayTheoMa(long maBaoCao, int maNguoiDung)
        {
            var baoCao = await baoCaoThangRepository.LayTheoMa(maBaoCao, maNguoiDung);
            if (baoCao != null) ChuanHoaGioUTC(baoCao);
            return baoCao;
        }

        public async Task<(IEnumerable<BaoCaoThang> DanhSach, int TongSoBanGhi)> LayDanhSach(int maNguoiDung, int trang, int kichThuocTrang)
        {
            if (trang < 1 || kichThuocTrang < 1 || kichThuocTrang > 100)
                throw new ArgumentException("Trang phải từ 1; kích thước trang từ 1 đến 100");
            var ketQua = await baoCaoThangRepository.LayDanhSach(maNguoiDung, trang, kichThuocTrang);
            var danhSach = ketQua.DanhSach.ToList();
            danhSach.ForEach(ChuanHoaGioUTC);
            return (danhSach, ketQua.TongSoBanGhi);
        }

        public Task<byte[]?> LayFile(long maBaoCao, int maNguoiDung)
        {
            return baoCaoThangRepository.LayFile(maBaoCao, maNguoiDung);
        }

        public async Task<bool> XuLyHangDoi(CancellationToken cancellationToken)
        {
            var baoCao = await baoCaoThangRepository.NhanYeuCau(cancellationToken);
            if (baoCao == null) return false;
            try
            {
                var duLieu = await baoCaoThangRepository.LayDuLieu(baoCao, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                byte[] filePDF = baoCaoPdfService.TaoFile(duLieu);
                if (filePDF.Length > 20 * 1024 * 1024) throw new ArgumentException("Báo cáo vượt giới hạn file PDF 20 MB");
                await baoCaoThangRepository.HoanThanh(baoCao, filePDF, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Giữ yêu cầu trong database; worker khác nhận lại sau khi hết thời hạn xử lý.
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Không tạo được báo cáo {MaBaoCao}, lần thử {SoLanThu}", baoCao.MaBaoCao, baoCao.SoLanThu);
                string thongBao = ex is ArgumentException ? ex.Message : "Không thể tạo báo cáo; hệ thống thử lại tối đa 3 lần. Nếu vẫn lỗi, hãy tạo yêu cầu mới";
                await baoCaoThangRepository.GhiLoi(baoCao, thongBao.Length > 500 ? thongBao[..500] : thongBao,
                    ex is ArgumentException, cancellationToken);
            }
            return true;
        }

        private static void ChuanHoaGioUTC(BaoCaoThang baoCao)
        {
            baoCao.NgayTao = DateTime.SpecifyKind(baoCao.NgayTao, DateTimeKind.Utc);
            if (baoCao.NgayBatDau.HasValue) baoCao.NgayBatDau = DateTime.SpecifyKind(baoCao.NgayBatDau.Value, DateTimeKind.Utc);
            if (baoCao.NgayHoanThanh.HasValue) baoCao.NgayHoanThanh = DateTime.SpecifyKind(baoCao.NgayHoanThanh.Value, DateTimeKind.Utc);
        }
    }
}
