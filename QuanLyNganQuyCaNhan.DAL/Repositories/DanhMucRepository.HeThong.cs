using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public partial class DanhMucRepository
    {
        public async Task<IEnumerable<DanhMuc>> LayDanhSachHeThong(string? loaiDanhMuc)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            return await ketNoiCSDL.QueryAsync<DanhMuc>(@"SELECT MaDanhMuc, MaNguoiDung, TenDanhMuc,
                LoaiDanhMuc, MoTa, NgayTao, NgayXoa FROM DanhMuc WHERE MaNguoiDung IS NULL AND NgayXoa IS NULL
                AND (@LoaiDanhMuc IS NULL OR LoaiDanhMuc = @LoaiDanhMuc) ORDER BY MaDanhMuc DESC", new { LoaiDanhMuc = loaiDanhMuc });
        }

        public async Task<DanhMuc?> LayHeThongTheoMa(int maDanhMuc)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            return await ketNoiCSDL.QuerySingleOrDefaultAsync<DanhMuc>(@"SELECT MaDanhMuc, MaNguoiDung, TenDanhMuc,
                LoaiDanhMuc, MoTa, NgayTao, NgayXoa FROM DanhMuc WHERE MaDanhMuc = @MaDanhMuc
                AND MaNguoiDung IS NULL AND NgayXoa IS NULL", new { MaDanhMuc = maDanhMuc });
        }

        public async Task<int> GhiDanhMucHeThong(DanhMuc danhMuc, bool capNhat)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            await ketNoiCSDL.OpenAsync();
            using var giaoDich = ketNoiCSDL.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                if (capNhat)
                {
                    string? loaiCu = await ketNoiCSDL.ExecuteScalarAsync<string?>(@"SELECT LoaiDanhMuc FROM DanhMuc WITH (UPDLOCK, HOLDLOCK)
                        WHERE MaDanhMuc = @MaDanhMuc AND MaNguoiDung IS NULL AND NgayXoa IS NULL", danhMuc, giaoDich);
                    if (loaiCu == null) { giaoDich.Rollback(); return 0; }
                    if (loaiCu != danhMuc.LoaiDanhMuc && await ketNoiCSDL.ExecuteScalarAsync<int>(@"SELECT CASE WHEN
                        EXISTS (SELECT 1 FROM GiaoDich WHERE MaDanhMuc = @MaDanhMuc)
                        OR EXISTS (SELECT 1 FROM NganSach WHERE MaDanhMuc = @MaDanhMuc) THEN 1 ELSE 0 END", danhMuc, giaoDich) == 1)
                        throw new ArgumentException("Không được đổi loại danh mục đã có giao dịch hoặc ngân sách");
                }
                if (await ketNoiCSDL.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM DanhMuc WITH (UPDLOCK, HOLDLOCK)
                    WHERE MaNguoiDung IS NULL AND TenDanhMuc = @TenDanhMuc AND NgayXoa IS NULL
                    AND MaDanhMuc <> @MaDanhMuc", danhMuc, giaoDich) > 0)
                    throw new ArgumentException("Tên danh mục hệ thống đã tồn tại");
                int ketQua = capNhat
                    ? await ketNoiCSDL.ExecuteAsync(@"UPDATE DanhMuc SET TenDanhMuc = @TenDanhMuc,
                        LoaiDanhMuc = @LoaiDanhMuc, MoTa = @MoTa WHERE MaDanhMuc = @MaDanhMuc
                        AND MaNguoiDung IS NULL AND NgayXoa IS NULL", danhMuc, giaoDich)
                    : await ketNoiCSDL.ExecuteScalarAsync<int>(@"INSERT INTO DanhMuc (MaNguoiDung, TenDanhMuc, LoaiDanhMuc, MoTa)
                        VALUES (NULL, @TenDanhMuc, @LoaiDanhMuc, @MoTa); SELECT CAST(SCOPE_IDENTITY() AS INT);", danhMuc, giaoDich);
                giaoDich.Commit();
                return ketQua;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                try { giaoDich.Rollback(); } catch { }
                throw new ArgumentException("Tên danh mục hệ thống đã tồn tại", ex);
            }
            catch { try { giaoDich.Rollback(); } catch { } throw; }
        }

        public async Task<int> XoaMemDanhMucHeThong(int maDanhMuc)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            return await ketNoiCSDL.ExecuteAsync(@"UPDATE DanhMuc SET NgayXoa = SYSDATETIME()
                WHERE MaDanhMuc = @MaDanhMuc AND MaNguoiDung IS NULL AND NgayXoa IS NULL", new { MaDanhMuc = maDanhMuc });
        }
    }
}
