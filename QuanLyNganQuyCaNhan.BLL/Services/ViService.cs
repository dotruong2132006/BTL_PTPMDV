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
    public class ViService : IViService
    {
        private readonly ViRepository viRepository;
        public ViService(ViRepository viRepository)
        {
            this.viRepository = viRepository;
        }
        public async Task<IEnumerable<Vi>> LayDanhSach(int maNguoiDung)
        {
            return await viRepository.LayDanhSach(maNguoiDung);
        }
        public async Task<int> ThemVi(Vi vi)
        {
            bool nguoiDungTonTai =await viRepository.KiemTraNguoiDungTonTai(vi.MaNguoiDung);
            if (!nguoiDungTonTai) throw new Exception("Người dùng không tồn tại");
            bool trungTenVi = await viRepository.KiemTraTenViTonTai(vi.MaNguoiDung,vi.TenVi);
            if (trungTenVi) throw new Exception("Tên ví đã tồn tại");
            return await viRepository.ThemVi(vi);
        }
        public async Task<bool> KiemTraTenViTonTai(int maNguoiDung,string tenVi)
        {
            return await viRepository.KiemTraTenViTonTai(maNguoiDung,tenVi);
        }

        public async Task<bool> KiemTraNguoiDungTonTai(int maNguoiDung)
        {
            return await viRepository.KiemTraNguoiDungTonTai( maNguoiDung);
        }
        public async Task<int> CapNhatVi(Vi vi)
        {
            bool nguoiDungTonTai = await viRepository.KiemTraNguoiDungTonTai(vi.MaNguoiDung);
            if (!nguoiDungTonTai) throw new Exception("Người dùng không tồn tại");
            bool trungTenVi = await viRepository.KiemTraTenViTrungKhiCapNhat(vi.MaNguoiDung,vi.TenVi,vi.MaVi);
            if (trungTenVi) throw new Exception("Tên ví đã được sử dụng");
            return await viRepository.CapNhatVi(vi);
        }
        public async Task<bool> KiemTraTenViTrungKhiCapNhat(int maNguoiDung,string tenVi,int maVi)
        {
            return await viRepository.KiemTraTenViTrungKhiCapNhat(maNguoiDung,tenVi,maVi);
        }
        public async Task<int> XoaMemVi(int maVi)
        {
            return await viRepository.XoaMemVi(maVi);
        }
    }
}
