using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class DatLaiMatKhauTokenRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;
        public DatLaiMatKhauTokenRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }
        public async Task<long> ThemToken(DatLaiMatKhauToken duLieu)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"INSERT INTO DatLaiMatKhauToken(MaNguoiDung,Token,NgayHetHan,DaSuDung,NgayTao)
                               VALUES (@MaNguoiDung,@Token,@NgayHetHan,@DaSuDung,@NgayTao);
                               SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            ";
            return await ketNoi.ExecuteScalarAsync<long>(cauLenh,duLieu);
        }
        public async Task<DatLaiMatKhauToken?> LayTheoToken(string token)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT MaDatLaiMatKhau,MaNguoiDung,Token,NgayHetHan,DaSuDung,NgayTao
                               FROM DatLaiMatKhauToken
                               WHERE Token = @Token
            ";
            return await ketNoi.QueryFirstOrDefaultAsync<DatLaiMatKhauToken>(cauLenh,new{Token = token});
        }

        public async Task<int> DanhDauDaSuDung(long maDatLaiMatKhau)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE DatLaiMatKhauToken
                               SET DaSuDung = 1
                               WHERE MaDatLaiMatKhau = @MaDatLaiMatKhau
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaDatLaiMatKhau =maDatLaiMatKhau});
        }
    }
}
