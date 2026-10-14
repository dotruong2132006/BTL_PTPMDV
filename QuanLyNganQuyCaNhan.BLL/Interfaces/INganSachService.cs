using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface INganSachService
    {
        Task<IEnumerable<TinhTrangNganSach>> LayDanhSach(int maNguoiDung, int? thang, int? nam, int? maDanhMuc);
        Task<TinhTrangNganSach?> LayTheoMa(int maNganSach, int maNguoiDung);
        Task<IEnumerable<TinhTrangNganSach>> LayCanhBao(int maNguoiDung, int thang, int nam);
        Task<IEnumerable<TinhTrangNganSach>> LayCanhBaoVuotNganSach(int maNguoiDung, int maDanhMuc, DateTime ngayGiaoDich);
        Task<int> ThemNganSach(NganSach nganSach);
        Task<int> CapNhatNganSach(NganSach nganSach);
        Task<int> XoaMemNganSach(int maNganSach, int maNguoiDung);
    }
}
