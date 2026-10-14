using Dapper;
using System.Globalization;
using System.Text.Json;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class MaKhoiPhucRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        public MaKhoiPhucRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<int> LaySoMaConLai(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM MaKhoiPhuc mkp
                INNER JOIN NguoiDung nd ON nd.MaNguoiDung = mkp.MaNguoiDung
                WHERE mkp.MaNguoiDung = @MaNguoiDung AND mkp.NgaySuDung IS NULL AND mkp.NgayVoHieu IS NULL
                  AND nd.TrangThai = 1 AND nd.NgayXoa IS NULL", new { MaNguoiDung = maNguoiDung });
        }

        public Task<bool> TaoBoMa(NguoiDung nguoiDung, IReadOnlyCollection<byte[]> cacMaDaBam, string? diaChiIP)
        {
            return GhiThayDoi(nguoiDung, cacMaDaBam, null, null, diaChiIP);
        }

        public Task<bool> DatLaiMatKhau(NguoiDung nguoiDung, byte[] maDaBam, string matKhauMoiDaBam, string? diaChiIP)
        {
            return GhiThayDoi(nguoiDung, null, maDaBam, matKhauMoiDaBam, diaChiIP);
        }

        private async Task<bool> GhiThayDoi(NguoiDung nguoiDung, IReadOnlyCollection<byte[]>? cacMaDaBam,
            byte[]? maDaBam, string? matKhauMoiDaBam, string? diaChiIP)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using var giaoDich = ketNoi.BeginTransaction();
            try
            {
                var hienTai = await ketNoi.QuerySingleOrDefaultAsync<NguoiDung>(
                    @"SELECT MaNguoiDung, MatKhau, DaBat2FA, MaBiMat2FA FROM NguoiDung WITH (UPDLOCK, HOLDLOCK)
                      WHERE MaNguoiDung = @MaNguoiDung AND TrangThai = 1 AND NgayXoa IS NULL",
                    new { nguoiDung.MaNguoiDung }, giaoDich);
                if (hienTai == null || hienTai.MatKhau != nguoiDung.MatKhau
                    || hienTai.DaBat2FA != nguoiDung.DaBat2FA
                    || (hienTai.MaBiMat2FA ?? "") != (nguoiDung.MaBiMat2FA ?? ""))
                {
                    giaoDich.Rollback();
                    return false;
                }

                bool taoBoMa = cacMaDaBam != null;
                if (taoBoMa)
                {
                    await ketNoi.ExecuteAsync(@"UPDATE MaKhoiPhuc SET NgayVoHieu = SYSUTCDATETIME()
                        WHERE MaNguoiDung = @MaNguoiDung AND NgayVoHieu IS NULL",
                        new { nguoiDung.MaNguoiDung }, giaoDich);
                    Guid maBoMa = Guid.NewGuid();
                    foreach (byte[] ma in cacMaDaBam!)
                        await ketNoi.ExecuteAsync(@"INSERT INTO MaKhoiPhuc (MaNguoiDung, MaBoMa, MaDaBam)
                            VALUES (@MaNguoiDung, @MaBoMa, @MaDaBam)",
                            new { nguoiDung.MaNguoiDung, MaBoMa = maBoMa, MaDaBam = ma }, giaoDich);
                }
                else
                {
                    int soDong = await ketNoi.ExecuteAsync(@"UPDATE MaKhoiPhuc SET NgaySuDung = SYSUTCDATETIME()
                        WHERE MaNguoiDung = @MaNguoiDung AND MaDaBam = @MaDaBam
                          AND NgaySuDung IS NULL AND NgayVoHieu IS NULL",
                        new { nguoiDung.MaNguoiDung, MaDaBam = maDaBam }, giaoDich);
                    if (soDong != 1)
                    {
                        giaoDich.Rollback();
                        return false;
                    }
                    await ketNoi.ExecuteAsync(@"UPDATE NguoiDung SET MatKhau = @MatKhauMoi, NgayCapNhat = SYSDATETIME()
                            WHERE MaNguoiDung = @MaNguoiDung;
                        UPDATE RefreshToken SET DaThuHoi = 1 WHERE MaNguoiDung = @MaNguoiDung AND DaThuHoi = 0;",
                        new { nguoiDung.MaNguoiDung, MatKhauMoi = matKhauMoiDaBam }, giaoDich);
                }

                await ketNoi.ExecuteAsync(@"INSERT INTO NhatKyAudit
                    (MaNguoiDung, HanhDong, TenBang, MaBanGhi, DuLieuMoi, DiaChiIP)
                    VALUES (@MaNguoiDung, @HanhDong, 'NguoiDung', @MaBanGhi, @DuLieuMoi, @DiaChiIP)",
                    new
                    {
                        nguoiDung.MaNguoiDung,
                        HanhDong = taoBoMa ? "TAO_MA_KHOI_PHUC" : "DAT_LAI_MAT_KHAU",
                        MaBanGhi = nguoiDung.MaNguoiDung.ToString(CultureInfo.InvariantCulture),
                        DuLieuMoi = taoBoMa ? JsonSerializer.Serialize(new { SoLuongMa = cacMaDaBam!.Count })
                            : JsonSerializer.Serialize(new { DaDatLaiMatKhau = true, DaThuHoiRefreshToken = true }),
                        DiaChiIP = diaChiIP
                    }, giaoDich);
                giaoDich.Commit();
                return true;
            }
            catch
            {
                try { giaoDich.Rollback(); } catch { }
                throw;
            }
        }
    }
}
