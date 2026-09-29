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
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly RefreshTokenRepository refreshTokenRepository;
        public RefreshTokenService(RefreshTokenRepository refreshTokenRepository)
        {
            this.refreshTokenRepository =refreshTokenRepository;
        }
        public async Task<long> ThemRefreshToken(RefreshToken refreshToken)
        {
            return await refreshTokenRepository.ThemRefreshToken(refreshToken);
        }
        public async Task<RefreshToken?> LayTheoToken(string token)
        {
            return await refreshTokenRepository.LayTheoToken(token);
        }
        public async Task<int> ThuHoiToken(long maRefreshToken)
        {
            return await refreshTokenRepository.ThuHoiToken(maRefreshToken);
        }
        public async Task<int> ThuHoiTatCaToken(int maNguoiDung)
        {
            return await refreshTokenRepository.ThuHoiTatCaToken(maNguoiDung);
        }
    }
}
