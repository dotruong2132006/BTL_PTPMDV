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
    public class NguoiDungService : INguoiDungService
    {
        private readonly NguoiDungRepository nguoiDungRepository;
        private readonly MatKhauService matKhauService;
        public NguoiDungService(NguoiDungRepository nguoiDungRepository,MatKhauService matKhauService)
        {
            this.nguoiDungRepository = nguoiDungRepository;
            this.matKhauService = matKhauService;
        }
        public async Task<IEnumerable<NguoiDung>> LayDanhSach()
        {
            return await nguoiDungRepository.LayDanhSach();
        }
        public async Task<int> ThemNguoiDung(NguoiDung nguoiDung)
        {
            bool trungTenDangNhap =await nguoiDungRepository.KiemTraTenDangNhapTonTai(nguoiDung.TenDangNhap);
            if (trungTenDangNhap)throw new Exception("Tên đăng nhập đã tồn tại");
            bool trungEmail =await nguoiDungRepository.KiemTraEmailTonTai(nguoiDung.Email);
            if (trungEmail) throw new Exception("Email đã tồn tại");
            nguoiDung.MatKhau = matKhauService.BamMatKhau(nguoiDung.MatKhau);
            return await nguoiDungRepository.ThemNguoiDung(nguoiDung);
        }
        public async Task<int> CapNhatNguoiDung(NguoiDung nguoiDung)
        {
            bool trungEmail =await nguoiDungRepository.KiemTraEmailTrungKhiCapNhat(nguoiDung.Email, nguoiDung.MaNguoiDung);
            if (trungEmail)
            {
                throw new Exception("Email đã được sử dụng bởi người dùng khác");
            }
            return await nguoiDungRepository.CapNhatNguoiDung(nguoiDung);
        }
        public async Task<bool> KiemTraTenDangNhapTonTai(string tenDangNhap)
        {
            return await nguoiDungRepository.KiemTraTenDangNhapTonTai(tenDangNhap);
        }
        public async Task<bool> KiemTraEmailTonTai(string email)
        {
            return await nguoiDungRepository.KiemTraEmailTonTai(email);
        }
        public async Task<bool> KiemTraEmailTrungKhiCapNhat(string email,int maNguoiDung)
        {
            return await nguoiDungRepository.KiemTraEmailTrungKhiCapNhat(email,maNguoiDung);
        }
        public async Task<int> XoaMemNguoiDung(int maNguoiDung)
        {
            return await nguoiDungRepository.XoaMemNguoiDung(maNguoiDung);
        }
        public async Task<NguoiDung?> LayTheoTenDangNhap(string tenDangNhap)
        {
            return await nguoiDungRepository.LayTheoTenDangNhap(tenDangNhap);
        }
        public async Task<string?> LayVaiTro(int maNguoiDung)
        {
            return await nguoiDungRepository.LayVaiTro(maNguoiDung);
        }
        public async Task<NguoiDung?> LayTheoMaNguoiDung(int maNguoiDung)
        {
            return await nguoiDungRepository.LayTheoMaNguoiDung(maNguoiDung);
        }
        public async Task<int> Bat2FA(int maNguoiDung,string maBiMat2FA)
        {
            return await nguoiDungRepository.Bat2FA(maNguoiDung,maBiMat2FA);
        }
        public async Task<int> Tat2FA(int maNguoiDung)
        {
            return await nguoiDungRepository.Tat2FA(maNguoiDung);
        }
        public async Task<int> CapNhatMatKhau(int maNguoiDung,string matKhauMoi)
        {
            string matKhauDaBam =matKhauService.BamMatKhau(matKhauMoi);
            return await nguoiDungRepository.CapNhatMatKhau(maNguoiDung,matKhauDaBam);
        }
    }
}
