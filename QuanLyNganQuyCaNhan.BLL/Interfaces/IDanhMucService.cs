using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IDanhMucService
    {
        Task<IEnumerable<DanhMuc>> LayDanhSach(int maNguoiDung);
        Task<int> ThemDanhMuc(DanhMuc danhMuc);
        Task<int> CapNhatDanhMuc(DanhMuc danhMuc);
        Task<int> XoaMemDanhMuc(int maDanhMuc, int maNguoiDung);
    }
}
