using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class DanhMucHeThongService : IDanhMucHeThongService
    {
        private readonly DanhMucRepository danhMucRepository;
        public DanhMucHeThongService(DanhMucRepository danhMucRepository) { this.danhMucRepository = danhMucRepository; }

        public Task<IEnumerable<DanhMuc>> LayDanhSach(string? loaiDanhMuc)
        {
            if (loaiDanhMuc != null && loaiDanhMuc != "THU" && loaiDanhMuc != "CHI")
                throw new ArgumentException("Loại danh mục chỉ được là THU hoặc CHI");
            return danhMucRepository.LayDanhSachHeThong(loaiDanhMuc);
        }

        public Task<DanhMuc?> LayTheoMa(int maDanhMuc) => danhMucRepository.LayHeThongTheoMa(maDanhMuc);
        public Task<int> ThemDanhMuc(DanhMuc danhMuc)
        {
            KiemTra(danhMuc);
            return danhMucRepository.GhiDanhMucHeThong(danhMuc, false);
        }
        public Task<int> CapNhatDanhMuc(DanhMuc danhMuc)
        {
            KiemTra(danhMuc);
            return danhMucRepository.GhiDanhMucHeThong(danhMuc, true);
        }
        public Task<int> XoaMemDanhMuc(int maDanhMuc) => danhMucRepository.XoaMemDanhMucHeThong(maDanhMuc);

        private static void KiemTra(DanhMuc danhMuc)
        {
            if (string.IsNullOrWhiteSpace(danhMuc.TenDanhMuc)) throw new ArgumentException("Tên danh mục không được để trống");
            danhMuc.TenDanhMuc = danhMuc.TenDanhMuc.Trim();
            if (danhMuc.TenDanhMuc.Length > 100) throw new ArgumentException("Tên danh mục tối đa 100 ký tự");
            if (danhMuc.MoTa?.Length > 255) throw new ArgumentException("Mô tả tối đa 255 ký tự");
            if (danhMuc.LoaiDanhMuc != "THU" && danhMuc.LoaiDanhMuc != "CHI") throw new ArgumentException("Loại danh mục chỉ được là THU hoặc CHI");
            danhMuc.MaNguoiDung = null;
        }
    }
}
