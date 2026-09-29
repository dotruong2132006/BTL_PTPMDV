using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.DAL.Models;


namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IViService
    {
        Task<IEnumerable<Vi>> LayDanhSach(int maNguoiDung);
        Task<int> ThemVi(Vi vi);
        Task<bool> KiemTraTenViTonTai(int maNguoiDung,string tenVi);
        Task<bool> KiemTraNguoiDungTonTai(int maNguoiDung);
        Task<int> CapNhatVi(Vi vi);
        Task<bool> KiemTraTenViTrungKhiCapNhat(int maNguoiDung, string tenVi, int maVi);
        Task<int> XoaMemVi(int maVi);
    }
}
