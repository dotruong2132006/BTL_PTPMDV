using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.DAL.Models;


namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IDatLaiMatKhauTokenService
    {
        Task<long> ThemToken(DatLaiMatKhauToken duLieu);
        Task<DatLaiMatKhauToken?> LayTheoToken(string token);
        Task<int> DanhDauDaSuDung(long maDatLaiMatKhau);
    }
}
