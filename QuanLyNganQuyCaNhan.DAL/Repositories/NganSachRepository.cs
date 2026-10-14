using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class NganSachRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        public NganSachRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<IEnumerable<TinhTrangNganSach>> LayDanhSach(
            int maNguoiDung, int? thang, int? nam, int? maDanhMuc, int? maNganSach = null)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT ns.MaNganSach, ns.MaNguoiDung, ns.MaDanhMuc,
                                      ns.SoTienGioiHan, ns.Thang, ns.Nam, ns.PhanTramCanhBao,
                                      ns.NgayTao, ns.NgayCapNhat, ns.NgayXoa, dm.TenDanhMuc,
                                      COALESCE(chi.TongChi, 0) AS TongChi
                               FROM NganSach ns
                               INNER JOIN DanhMuc dm ON dm.MaDanhMuc = ns.MaDanhMuc
                                   AND (dm.MaNguoiDung = ns.MaNguoiDung OR dm.MaNguoiDung IS NULL)
                               OUTER APPLY
                               (
                                   SELECT SUM(gd.SoTien) AS TongChi
                                   FROM GiaoDich gd
                                   WHERE gd.MaNguoiDung = ns.MaNguoiDung
                                     AND gd.MaDanhMuc = ns.MaDanhMuc
                                     AND gd.LoaiGiaoDich = 'CHI'
                                     AND gd.NgayXoa IS NULL
                                     AND gd.NgayGiaoDich >= DATEFROMPARTS(ns.Nam, ns.Thang, 1)
                                     AND gd.NgayGiaoDich < DATEADD(MONTH, 1, DATEFROMPARTS(ns.Nam, ns.Thang, 1))
                               ) chi
                               WHERE ns.MaNguoiDung = @MaNguoiDung AND ns.NgayXoa IS NULL
                                 AND (@Thang IS NULL OR ns.Thang = @Thang)
                                 AND (@Nam IS NULL OR ns.Nam = @Nam)
                                 AND (@MaDanhMuc IS NULL OR ns.MaDanhMuc = @MaDanhMuc)
                                 AND (@MaNganSach IS NULL OR ns.MaNganSach = @MaNganSach)
                               ORDER BY ns.Nam DESC, ns.Thang DESC, ns.MaNganSach DESC";
            return await ketNoi.QueryAsync<TinhTrangNganSach>(cauLenh,
                new { MaNguoiDung = maNguoiDung, Thang = thang, Nam = nam,
                    MaDanhMuc = maDanhMuc, MaNganSach = maNganSach });
        }

        public async Task<int> ThemNganSach(NganSach nganSach)
        {
            return await GhiNganSach(nganSach, false);
        }

        public async Task<int> CapNhatNganSach(NganSach nganSach)
        {
            return await GhiNganSach(nganSach, true);
        }

        private async Task<int> GhiNganSach(NganSach nganSach, bool capNhat)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using IDbTransaction giaoDichCSDL = ketNoi.BeginTransaction();
            try
            {
                if (capNhat)
                {
                    string kiemTra = @"SELECT COUNT(1) FROM NganSach WITH (UPDLOCK, HOLDLOCK)
                                        WHERE MaNganSach = @MaNganSach
                                          AND MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL";
                    if (await ketNoi.ExecuteScalarAsync<int>(kiemTra, nganSach, giaoDichCSDL) == 0)
                    {
                        giaoDichCSDL.Rollback();
                        return 0;
                    }
                }

                string kiemTraNguoiDung = @"SELECT COUNT(1) FROM NguoiDung WITH (HOLDLOCK)
                                            WHERE MaNguoiDung = @MaNguoiDung
                                              AND TrangThai = 1 AND NgayXoa IS NULL";
                if (await ketNoi.ExecuteScalarAsync<int>(kiemTraNguoiDung, nganSach, giaoDichCSDL) == 0)
                    throw new ArgumentException("Người dùng không tồn tại hoặc đã bị khóa");

                string kiemTraDanhMuc = @"SELECT COUNT(1) FROM DanhMuc WITH (HOLDLOCK)
                                          WHERE MaDanhMuc = @MaDanhMuc AND (MaNguoiDung = @MaNguoiDung OR MaNguoiDung IS NULL)
                                            AND LoaiDanhMuc = 'CHI' AND NgayXoa IS NULL";
                if (await ketNoi.ExecuteScalarAsync<int>(kiemTraDanhMuc, nganSach, giaoDichCSDL) == 0)
                    throw new ArgumentException("Ngân sách chỉ áp dụng cho danh mục CHI cá nhân hoặc hệ thống chưa xóa");

                string cauLenh = capNhat
                    ? @"UPDATE NganSach SET MaDanhMuc = @MaDanhMuc, SoTienGioiHan = @SoTienGioiHan,
                            Thang = @Thang, Nam = @Nam, PhanTramCanhBao = @PhanTramCanhBao,
                            NgayCapNhat = SYSDATETIME()
                        WHERE MaNganSach = @MaNganSach AND MaNguoiDung = @MaNguoiDung
                          AND NgayXoa IS NULL"
                    : @"INSERT INTO NganSach (MaNguoiDung, MaDanhMuc, SoTienGioiHan, Thang, Nam, PhanTramCanhBao)
                        VALUES (@MaNguoiDung, @MaDanhMuc, @SoTienGioiHan, @Thang, @Nam, @PhanTramCanhBao);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                int ketQua = capNhat
                    ? await ketNoi.ExecuteAsync(cauLenh, nganSach, giaoDichCSDL)
                    : await ketNoi.ExecuteScalarAsync<int>(cauLenh, nganSach, giaoDichCSDL);
                giaoDichCSDL.Commit();
                return ketQua;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw new ArgumentException("Danh mục đã có ngân sách trong tháng và năm này", ex);
            }
            catch
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw;
            }
        }

        public async Task<int> XoaMemNganSach(int maNganSach, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE NganSach SET NgayXoa = SYSDATETIME(), NgayCapNhat = SYSDATETIME()
                               WHERE MaNganSach = @MaNganSach AND MaNguoiDung = @MaNguoiDung
                                 AND NgayXoa IS NULL";
            return await ketNoi.ExecuteAsync(cauLenh, new { MaNganSach = maNganSach, MaNguoiDung = maNguoiDung });
        }
    }
}
