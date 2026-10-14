using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public partial class GiaoDichRepository
    {
        public async Task<IEnumerable<GiaoDich>> LayDuLieuXuat(int maNguoiDung, DateTime? tuNgay, DateTime? denNgay,
            int? maDanhMuc, int? maVi)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QueryAsync<GiaoDich>(@"SELECT TOP (10001) MaGiaoDich, MaVi, MaDanhMuc,
                    LoaiGiaoDich, SoTien, NgayGiaoDich, NoiDung FROM GiaoDich
                WHERE MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL
                  AND (@TuNgay IS NULL OR NgayGiaoDich >= @TuNgay)
                  AND (@DenNgay IS NULL OR NgayGiaoDich < @DenNgay)
                  AND (@MaDanhMuc IS NULL OR MaDanhMuc = @MaDanhMuc)
                  AND (@MaVi IS NULL OR MaVi = @MaVi)
                ORDER BY NgayGiaoDich, MaGiaoDich",
                new { MaNguoiDung = maNguoiDung, TuNgay = tuNgay, DenNgay = denNgay, MaDanhMuc = maDanhMuc, MaVi = maVi });
        }

        public async Task<KetQuaNhapGiaoDich> NhapGiaoDich(int maNguoiDung, Guid maYeuCau, byte[] hashFile,
            IReadOnlyList<GiaoDich> cacGiaoDich, string? diaChiIP)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using IDbTransaction giaoDichCSDL = ketNoi.BeginTransaction(IsolationLevel.ReadCommitted);
            int dongHienTai = 0;
            try
            {
                var lanCu = await ketNoi.QuerySingleOrDefaultAsync(
                    @"SELECT MaLanNhap, SoDong, HashFile FROM LanNhapGiaoDich WITH (UPDLOCK, HOLDLOCK)
                        WHERE MaNguoiDung = @MaNguoiDung AND MaYeuCau = @MaYeuCau",
                    new { MaNguoiDung = maNguoiDung, MaYeuCau = maYeuCau }, giaoDichCSDL);
                if (lanCu != null)
                {
                    if (!hashFile.SequenceEqual((byte[])lanCu.HashFile))
                        throw new ArgumentException("Mã yêu cầu đã dùng cho file khác. Dùng lại đúng file khi thử lại hoặc tạo mã yêu cầu mới cho lần nhập khác");
                    giaoDichCSDL.Commit();
                    return new KetQuaNhapGiaoDich { MaLanNhap = (long)lanCu.MaLanNhap, SoDong = (int)lanCu.SoDong, DaNhapTruoc = true };
                }

                long maLanNhap = await ketNoi.ExecuteScalarAsync<long>(@"INSERT INTO LanNhapGiaoDich
                    (MaNguoiDung, MaYeuCau, HashFile, SoDong) VALUES (@MaNguoiDung, @MaYeuCau, @HashFile, @SoDong);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);",
                    new { MaNguoiDung = maNguoiDung, MaYeuCau = maYeuCau, HashFile = hashFile, SoDong = cacGiaoDich.Count }, giaoDichCSDL);

                if (!await KiemTraGiaoDichHopLeTrongGiaoDich(ketNoi, giaoDichCSDL,
                    cacGiaoDich[0], cacGiaoDich.Select(x => x.MaVi)))
                    throw new ArgumentException("Ví, danh mục hoặc tài khoản không hợp lệ trong file nhập");

                foreach (var giaoDich in cacGiaoDich)
                {
                    dongHienTai++;
                    if (!await KiemTraGiaoDichHopLeTrongGiaoDich(ketNoi, giaoDichCSDL, giaoDich, new[] { giaoDich.MaVi }))
                        throw new ArgumentException($"Bản ghi {dongHienTai}: ví/danh mục không thuộc người dùng, không hoạt động hoặc không khớp loại giao dịch");
                    await CapNhatSoDuVi(ketNoi, giaoDichCSDL, maNguoiDung, giaoDich.MaVi,
                        TinhBienDong(giaoDich.LoaiGiaoDich, giaoDich.SoTien));
                    long maGiaoDich = await ketNoi.ExecuteScalarAsync<long>(@"INSERT INTO GiaoDich
                        (MaNguoiDung, MaVi, MaDanhMuc, LoaiGiaoDich, SoTien, NoiDung, NgayGiaoDich)
                        VALUES (@MaNguoiDung, @MaVi, @MaDanhMuc, @LoaiGiaoDich, @SoTien, @NoiDung, @NgayGiaoDich);
                        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", giaoDich, giaoDichCSDL);
                    await ketNoi.ExecuteAsync(@"INSERT INTO NhatKyAudit (MaNguoiDung, HanhDong, TenBang, MaBanGhi, DuLieuMoi, DiaChiIP)
                        VALUES (@MaNguoiDung, 'NHAP_FILE', 'GiaoDich', @MaBanGhi, @DuLieuMoi, @DiaChiIP)",
                        new { MaNguoiDung = maNguoiDung, MaBanGhi = maGiaoDich.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            DuLieuMoi = JsonSerializer.Serialize(new { MaLanNhap = maLanNhap, Dong = dongHienTai,
                                giaoDich.MaVi, giaoDich.MaDanhMuc, giaoDich.LoaiGiaoDich, giaoDich.SoTien, giaoDich.NgayGiaoDich, giaoDich.NoiDung }), DiaChiIP = diaChiIP }, giaoDichCSDL);
                }
                giaoDichCSDL.Commit();
                return new KetQuaNhapGiaoDich { MaLanNhap = maLanNhap, SoDong = cacGiaoDich.Count };
            }
            catch (SqlException ex) when (ex.Number == 547 || ex.Number == 8115)
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw new ArgumentException($"Bản ghi {dongHienTai}: số dư hoặc dữ liệu vượt ràng buộc database; toàn bộ lần nhập đã được hoàn tác", ex);
            }
            catch
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw;
            }
        }
    }
}
