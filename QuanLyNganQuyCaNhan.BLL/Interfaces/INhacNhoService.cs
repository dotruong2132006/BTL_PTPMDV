using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface INhacNhoService
    {
        Task<IEnumerable<NhacNho>> LayDanhSach(int maNguoiDung, bool? trangThai);
        Task<NhacNho?> LayTheoMa(int maNhacNho, int maNguoiDung);
        Task<int> ThemNhacNho(NhacNho nhacNho);
        Task<int> CapNhatNhacNho(NhacNho nhacNho);
        Task<int> XoaMemNhacNho(int maNhacNho, int maNguoiDung);
        Task XuLyDenHan();
        Task<(IEnumerable<ThongBaoNhacNho> DanhSach, int TongSoBanGhi)> LayThongBao(int maNguoiDung, bool chiChuaDoc, int trang, int kichThuocTrang);
        Task<int> DanhDauDaDoc(long maThongBao, int maNguoiDung);
    }
}
