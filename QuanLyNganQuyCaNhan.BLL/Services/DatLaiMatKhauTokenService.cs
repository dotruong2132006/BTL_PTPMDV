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
    public class DatLaiMatKhauTokenService: IDatLaiMatKhauTokenService
    {
        private readonly DatLaiMatKhauTokenRepository datLaiMatKhauTokenRepository;
        public DatLaiMatKhauTokenService(DatLaiMatKhauTokenRepository datLaiMatKhauTokenRepository)
        {
            this.datLaiMatKhauTokenRepository =datLaiMatKhauTokenRepository;
        }
        public async Task<long> ThemToken(DatLaiMatKhauToken duLieu)
        {
            return await datLaiMatKhauTokenRepository.ThemToken(duLieu);
        }
        public async Task<DatLaiMatKhauToken?>LayTheoToken(string token)
        {
            return await datLaiMatKhauTokenRepository.LayTheoToken(token);
        }
        public async Task<int> DanhDauDaSuDung(long maDatLaiMatKhau)
        {
            return await datLaiMatKhauTokenRepository.DanhDauDaSuDung(maDatLaiMatKhau);
        }
    }
}

