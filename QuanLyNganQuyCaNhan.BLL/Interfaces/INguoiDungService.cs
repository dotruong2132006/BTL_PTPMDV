using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.DAL.Models;


namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface INguoiDungService
    {
        Task<IEnumerable<NguoiDung>> LayDanhSach();
        Task<int> ThemNguoiDung(NguoiDung nguoiDung);
        Task<bool> KiemTraTenDangNhapTonTai(string tenDangNhap);
        Task<bool> KiemTraEmailTonTai(string email);
        Task<int> CapNhatNguoiDung(NguoiDung nguoiDung);
        Task<bool> KiemTraEmailTrungKhiCapNhat(string email,int maNguoiDung);
        Task<int> XoaMemNguoiDung(int maNguoiDung);
        Task<NguoiDung?> LayTheoTenDangNhap(string tenDangNhap);
        Task<string?> LayVaiTro(int maNguoiDung);
        Task<NguoiDung?> LayTheoMaNguoiDung(int maNguoiDung);
        Task<int> Bat2FA(int maNguoiDung,string maBiMat2FA);
        Task<int> Tat2FA(int maNguoiDung);
        Task<int> CapNhatMatKhau(int maNguoiDung,string matKhauMoi
);
    }
}
