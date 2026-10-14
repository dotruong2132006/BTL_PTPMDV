using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IDanhMucHeThongService
    {
        Task<IEnumerable<DanhMuc>> LayDanhSach(string? loaiDanhMuc);
        Task<DanhMuc?> LayTheoMa(int maDanhMuc);
        Task<int> ThemDanhMuc(DanhMuc danhMuc);
        Task<int> CapNhatDanhMuc(DanhMuc danhMuc);
        Task<int> XoaMemDanhMuc(int maDanhMuc);
    }
}
