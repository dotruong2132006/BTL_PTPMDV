using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IGiaoDichService
    {
        Task<(IEnumerable<GiaoDich> DanhSach, int TongSoBanGhi)> LayDanhSach(
            int maNguoiDung,
            DateTime? tuNgay,
            DateTime? denNgay,
            int? maDanhMuc,
            int trang,
            int kichThuocTrang,
            string sapXepTheo,
            string chieuSapXep);

        Task<GiaoDich?> LayTheoMa(long maGiaoDich, int maNguoiDung);
        Task<long> ThemGiaoDich(GiaoDich giaoDich);
        Task<int> CapNhatGiaoDich(GiaoDich giaoDich);
        Task<int> XoaMemGiaoDich(long maGiaoDich, int maNguoiDung);
    }
}
