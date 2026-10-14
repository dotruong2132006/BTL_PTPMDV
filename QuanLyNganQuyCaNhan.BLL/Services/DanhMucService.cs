using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class DanhMucService : IDanhMucService
    {
        private readonly DanhMucRepository danhMucRepository;
        public DanhMucService(DanhMucRepository danhMucRepository)
        {
            this.danhMucRepository = danhMucRepository;
        }
        public async Task<IEnumerable<DanhMuc>> LayDanhSach(int maNguoiDung)
        {
            return await danhMucRepository.LayDanhSach(maNguoiDung);
        }
        public async Task<int> ThemDanhMuc(DanhMuc danhMuc)
        {
            danhMuc.TenDanhMuc = KiemTraVaChuanHoaTen(danhMuc.TenDanhMuc);

            if (!danhMuc.MaNguoiDung.HasValue) throw new ArgumentException("Không xác định được người dùng");
            bool nguoiDungTonTai =await danhMucRepository.KiemTraNguoiDungTonTai(danhMuc.MaNguoiDung.Value);
            if (!nguoiDungTonTai)
            {
                throw new Exception("Người dùng không tồn tại");
            }
            bool trungTen =await danhMucRepository.KiemTraTenDanhMucTonTai(danhMuc.MaNguoiDung.Value,danhMuc.TenDanhMuc);
            if (trungTen)
            {
                throw new Exception("Tên danh mục đã tồn tại");
            }
            if (danhMuc.LoaiDanhMuc != "THU" &&danhMuc.LoaiDanhMuc != "CHI")
            {
                throw new Exception("Loại danh mục chỉ được là THU hoặc CHI");
            }
            return await danhMucRepository.ThemDanhMuc(danhMuc);
        }
        public async Task<int> CapNhatDanhMuc(DanhMuc danhMuc)
        {
            danhMuc.TenDanhMuc = KiemTraVaChuanHoaTen(danhMuc.TenDanhMuc);

            if (!danhMuc.MaNguoiDung.HasValue) throw new ArgumentException("Không xác định được người dùng");
            bool nguoiDungTonTai =await danhMucRepository.KiemTraNguoiDungTonTai(danhMuc.MaNguoiDung.Value);
            if (!nguoiDungTonTai)
            {
                throw new Exception("Người dùng không tồn tại");
            }
            bool trungTen =await danhMucRepository.KiemTraTenDanhMucTonTai(danhMuc.MaNguoiDung.Value,danhMuc.TenDanhMuc,danhMuc.MaDanhMuc); if (trungTen)
            {
                throw new Exception("Tên danh mục đã tồn tại");
            }
            if (danhMuc.LoaiDanhMuc != "THU" &&danhMuc.LoaiDanhMuc != "CHI")
            {
                throw new Exception("Loại danh mục chỉ được là THU hoặc CHI");
            }
            return await danhMucRepository.CapNhatDanhMuc(danhMuc);
        }

        private static string KiemTraVaChuanHoaTen(string tenDanhMuc)
        {
            if (string.IsNullOrWhiteSpace(tenDanhMuc))
            {
                throw new Exception("Tên danh mục không được để trống");
            }

            return tenDanhMuc.Trim();
        }

        public async Task<int> XoaMemDanhMuc(int maDanhMuc, int maNguoiDung)
        {
            return await danhMucRepository.XoaMemDanhMuc(maDanhMuc, maNguoiDung);
        }
    }
}
