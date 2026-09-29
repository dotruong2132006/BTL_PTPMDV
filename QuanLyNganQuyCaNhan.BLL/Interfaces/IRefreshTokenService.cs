using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.DAL.Models;


namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<long> ThemRefreshToken(RefreshToken refreshToken);

        Task<RefreshToken?> LayTheoToken(string token);

        Task<int> ThuHoiToken(long maRefreshToken);
        Task<int> ThuHoiTatCaToken(int maNguoiDung);
    }
}
