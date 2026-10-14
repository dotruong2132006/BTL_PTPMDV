using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IBaoCaoThangService
    {
        Task<BaoCaoThang> TaoYeuCau(int maNguoiDung, int thang, int nam, Guid maYeuCau, string? diaChiIP);
        Task<BaoCaoThang?> LayTheoMa(long maBaoCao, int maNguoiDung);
        Task<(IEnumerable<BaoCaoThang> DanhSach, int TongSoBanGhi)> LayDanhSach(int maNguoiDung, int trang, int kichThuocTrang);
        Task<byte[]?> LayFile(long maBaoCao, int maNguoiDung);
        Task<bool> XuLyHangDoi(CancellationToken cancellationToken);
    }
}
