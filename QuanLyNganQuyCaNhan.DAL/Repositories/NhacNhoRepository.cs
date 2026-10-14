using Dapper;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class NhacNhoRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;
        private const string CacCot = "MaNhacNho, MaNguoiDung, TieuDe, NoiDung, NgayNhac, LapLai, KieuLapLai, TrangThai, NgayNhacTiepTheo, PhienBan, NgayTao, NgayCapNhat, NgayXoa";

        public NhacNhoRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<IEnumerable<NhacNho>> LayDanhSach(int maNguoiDung, bool? trangThai)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QueryAsync<NhacNho>($@"SELECT {CacCot} FROM NhacNho
                WHERE MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL
                    AND (@TrangThai IS NULL OR TrangThai = @TrangThai) ORDER BY NgayNhac, MaNhacNho",
                new { MaNguoiDung = maNguoiDung, TrangThai = trangThai });
        }

        public async Task<NhacNho?> LayTheoMa(int maNhacNho, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QuerySingleOrDefaultAsync<NhacNho>($@"SELECT {CacCot} FROM NhacNho
                WHERE MaNhacNho = @MaNhacNho AND MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL",
                new { MaNhacNho = maNhacNho, MaNguoiDung = maNguoiDung });
        }

        public async Task<int> ThemNhacNho(NhacNho nhacNho)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteScalarAsync<int>(@"INSERT INTO NhacNho
                (MaNguoiDung, TieuDe, NoiDung, NgayNhac, LapLai, KieuLapLai, TrangThai, NgayNhacTiepTheo, NgayTao)
                SELECT @MaNguoiDung, @TieuDe, @NoiDung, @NgayNhac, @LapLai, @KieuLapLai, @TrangThai, @NgayNhacTiepTheo, SYSUTCDATETIME()
                WHERE EXISTS (SELECT 1 FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung AND TrangThai = 1 AND NgayXoa IS NULL);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", nhacNho);
        }

        public async Task<int> CapNhatNhacNho(NhacNho nhacNho)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteAsync(@"UPDATE NhacNho SET TieuDe = @TieuDe, NoiDung = @NoiDung,
                NgayNhac = @NgayNhac, LapLai = @LapLai, KieuLapLai = @KieuLapLai, TrangThai = @TrangThai,
                NgayNhacTiepTheo = @NgayNhacTiepTheo, PhienBan = PhienBan + 1, NgayCapNhat = SYSUTCDATETIME()
                WHERE MaNhacNho = @MaNhacNho AND MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL AND PhienBan = @PhienBan
                  AND EXISTS (SELECT 1 FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung AND TrangThai = 1 AND NgayXoa IS NULL)", nhacNho);
        }

        public async Task<int> XoaMemNhacNho(int maNhacNho, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteAsync(@"UPDATE NhacNho SET TrangThai = 0, NgayNhacTiepTheo = NULL,
                NgayXoa = SYSUTCDATETIME(), NgayCapNhat = SYSUTCDATETIME(), PhienBan = PhienBan + 1
                WHERE MaNhacNho = @MaNhacNho AND MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL",
                new { MaNhacNho = maNhacNho, MaNguoiDung = maNguoiDung });
        }

        public async Task<IEnumerable<NhacNho>> LayDanhSachDenHan(DateTime hienTai)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QueryAsync<NhacNho>($@"SELECT TOP (100) {CacCot} FROM NhacNho
                WHERE TrangThai = 1 AND NgayXoa IS NULL AND NgayNhacTiepTheo <= @HienTai
                  AND EXISTS (SELECT 1 FROM NguoiDung nd WHERE nd.MaNguoiDung = NhacNho.MaNguoiDung AND nd.TrangThai = 1 AND nd.NgayXoa IS NULL)
                ORDER BY NgayNhacTiepTheo, MaNhacNho", new { HienTai = hienTai });
        }

        public async Task PhatThongBao(NhacNho nhacNho, DateTime ngayDenHan, DateTime? ngayTiepTheo)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using var giaoDich = ketNoi.BeginTransaction();
            try
            {
                int soLuong = await ketNoi.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM NhacNho WITH (UPDLOCK, HOLDLOCK)
                    WHERE MaNhacNho = @MaNhacNho AND MaNguoiDung = @MaNguoiDung AND TrangThai = 1 AND NgayXoa IS NULL
                        AND PhienBan = @PhienBan AND NgayNhacTiepTheo = @NgayNhacTiepTheo
                        AND EXISTS (SELECT 1 FROM NguoiDung nd WHERE nd.MaNguoiDung = @MaNguoiDung AND nd.TrangThai = 1 AND nd.NgayXoa IS NULL)", nhacNho, giaoDich);
                if (soLuong == 0)
                {
                    giaoDich.Rollback();
                    return;
                }
                await ketNoi.ExecuteAsync(@"INSERT INTO ThongBaoNhacNho (MaNhacNho, MaNguoiDung, TieuDe, NoiDung, NgayDenHan)
                    SELECT @MaNhacNho, @MaNguoiDung, @TieuDe, @NoiDung, @NgayDenHan
                    WHERE NOT EXISTS (SELECT 1 FROM ThongBaoNhacNho WHERE MaNhacNho = @MaNhacNho AND NgayDenHan = @NgayDenHan);
                    UPDATE NhacNho SET NgayNhacTiepTheo = @NgayTiepTheo, TrangThai = @TrangThai,
                        NgayCapNhat = SYSUTCDATETIME(), PhienBan = PhienBan + 1 WHERE MaNhacNho = @MaNhacNho;",
                    new { nhacNho.MaNhacNho, nhacNho.MaNguoiDung, nhacNho.TieuDe, nhacNho.NoiDung,
                        NgayDenHan = ngayDenHan, NgayTiepTheo = ngayTiepTheo, TrangThai = ngayTiepTheo.HasValue }, giaoDich);
                giaoDich.Commit();
            }
            catch
            {
                try { giaoDich.Rollback(); } catch { }
                throw;
            }
        }

        public async Task<(IEnumerable<ThongBaoNhacNho> DanhSach, int TongSoBanGhi)> LayThongBao(
            int maNguoiDung, bool chiChuaDoc, int trang, int kichThuocTrang)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1) FROM ThongBaoNhacNho
                    WHERE MaNguoiDung = @MaNguoiDung AND (@ChiChuaDoc = 0 OR NgayDoc IS NULL);
                SELECT MaThongBao, MaNhacNho, MaNguoiDung, TieuDe, NoiDung, NgayDenHan, NgayTao, NgayDoc FROM ThongBaoNhacNho
                    WHERE MaNguoiDung = @MaNguoiDung AND (@ChiChuaDoc = 0 OR NgayDoc IS NULL)
                    ORDER BY MaThongBao DESC OFFSET @ViTri ROWS FETCH NEXT @KichThuocTrang ROWS ONLY;";
            using var ketQua = await ketNoi.QueryMultipleAsync(cauLenh,
                new { MaNguoiDung = maNguoiDung, ChiChuaDoc = chiChuaDoc, ViTri = (long)(trang - 1) * kichThuocTrang, KichThuocTrang = kichThuocTrang });
            int tong = await ketQua.ReadSingleAsync<int>();
            return (await ketQua.ReadAsync<ThongBaoNhacNho>(), tong);
        }

        public async Task<int> DanhDauDaDoc(long maThongBao, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteAsync(@"UPDATE ThongBaoNhacNho SET NgayDoc = COALESCE(NgayDoc, SYSUTCDATETIME())
                WHERE MaThongBao = @MaThongBao AND MaNguoiDung = @MaNguoiDung",
                new { MaThongBao = maThongBao, MaNguoiDung = maNguoiDung });
        }
    }
}
