using Dapper;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        public RefreshTokenRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<long> ThemRefreshToken(RefreshToken refreshToken)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"INSERT INTO RefreshToken(MaNguoiDung,Token,NgayHetHan,DaThuHoi,NgayTao)
                               VALUES (@MaNguoiDung,@Token,@NgayHetHan,@DaThuHoi,@NgayTao);
                               SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            ";
            return await ketNoi.ExecuteScalarAsync<long>(cauLenh,refreshToken);
        }
        public async Task<RefreshToken?> LayTheoToken(string token)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT MaRefreshToken,MaNguoiDung,Token,NgayHetHan,DaThuHoi,NgayTao
                               FROM RefreshToken
                               WHERE Token = @Token
            ";
            return await ketNoi.QueryFirstOrDefaultAsync<RefreshToken>(cauLenh,new{Token = token});
        }
        public async Task<int> ThuHoiToken(long maRefreshToken)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE RefreshToken
                               SET DaThuHoi = 1
                               WHERE MaRefreshToken = @MaRefreshToken
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaRefreshToken = maRefreshToken});
        }
        public async Task<int> ThuHoiTatCaToken(int maNguoiDung)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE RefreshToken
                               SET DaThuHoi = 1
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND DaThuHoi = 0
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaNguoiDung = maNguoiDung});
        }
    }
}