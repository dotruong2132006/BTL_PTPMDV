using Dapper;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class NhatKyAuditRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        private static readonly Dictionary<string, (string TenBang, string CotKhoa, string CacCot, bool TheoNguoiDung)> BangDuocGhi = new(StringComparer.OrdinalIgnoreCase)
        {
            ["GiaoDich"] = ("GiaoDich", "MaGiaoDich", "MaGiaoDich, MaNguoiDung, MaVi, MaDanhMuc, LoaiGiaoDich, SoTien, NoiDung, NgayGiaoDich, PhienBan, NgayTao, NgayCapNhat, NgayXoa", true),
            ["DanhMuc"] = ("DanhMuc", "MaDanhMuc", "MaDanhMuc, MaNguoiDung, TenDanhMuc, LoaiDanhMuc, MoTa, NgayTao, NgayXoa", true),
            ["Vi"] = ("Vi", "MaVi", "MaVi, MaNguoiDung, TenVi, SoDu, MoTa, TrangThai, NgayTao, NgayCapNhat, NgayXoa", true),
            ["NganSach"] = ("NganSach", "MaNganSach", "MaNganSach, MaNguoiDung, MaDanhMuc, SoTienGioiHan, Thang, Nam, PhanTramCanhBao, NgayTao, NgayCapNhat, NgayXoa", true),
            ["NhacNho"] = ("NhacNho", "MaNhacNho", "MaNhacNho, MaNguoiDung, TieuDe, NoiDung, NgayNhac, LapLai, KieuLapLai, TrangThai, NgayNhacTiepTheo, PhienBan, NgayTao, NgayCapNhat, NgayXoa", true),
            ["NguoiDung"] = ("NguoiDung", "MaNguoiDung", "MaNguoiDung, TenDangNhap, HoTen, TrangThai, NgayTao, NgayCapNhat, NgayXoa", false)
        };

        public NhatKyAuditRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<string?> LayDuLieuHienTai(string tenBang, long maBanGhi, int? maNguoiDung)
        {
            if (!BangDuocGhi.TryGetValue(tenBang, out var cauHinhBang))
                return null;
            if (cauHinhBang.TheoNguoiDung && !maNguoiDung.HasValue)
                return null;

            string dieuKienNguoiDung = cauHinhBang.TheoNguoiDung? "AND MaNguoiDung = @MaNguoiDung": string.Empty;
            if (tenBang.Equals("DanhMuc", StringComparison.OrdinalIgnoreCase))
                dieuKienNguoiDung = "AND (MaNguoiDung = @MaNguoiDung OR MaNguoiDung IS NULL)";
            string cauLenh = $@"SELECT (
                                   SELECT {cauHinhBang.CacCot}
                                   FROM {cauHinhBang.TenBang}
                                   WHERE {cauHinhBang.CotKhoa} = @MaBanGhi
                                   {dieuKienNguoiDung}
                                   FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                               )";

            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteScalarAsync<string?>(cauLenh, new{MaBanGhi = maBanGhi,MaNguoiDung = maNguoiDung});
        }

        public async Task ThemNhatKy(NhatKyAudit nhatKy)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"INSERT INTO NhatKyAudit (MaNguoiDung, HanhDong, TenBang, MaBanGhi, DuLieuCu, DuLieuMoi, DiaChiIP)
                               VALUES (@MaNguoiDung, @HanhDong, @TenBang, @MaBanGhi, @DuLieuCu, @DuLieuMoi, @DiaChiIP)";
            await ketNoi.ExecuteAsync(cauLenh, nhatKy);
        }

        public async Task<(IEnumerable<NhatKyAudit> DanhSach, long TongSoBanGhi)> LayDanhSach(BoLocNhatKyAudit boLoc)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using var giaoDich = ketNoi.BeginTransaction(System.Data.IsolationLevel.Serializable);
            string dieuKien = @"(@MaNguoiDung IS NULL OR MaNguoiDung = @MaNguoiDung)
                AND (@TenBang IS NULL OR TenBang = @TenBang)
                AND (@HanhDong IS NULL OR HanhDong = @HanhDong)
                AND (@MaBanGhi IS NULL OR MaBanGhi = @MaBanGhi)
                AND (@TuNgay IS NULL OR ThoiGian >= @TuNgay)
                AND (@DenNgay IS NULL OR ThoiGian < @DenNgay)";
            string sql = $@"SELECT COUNT_BIG(*) FROM NhatKyAudit WHERE {dieuKien};
                SELECT MaAudit, MaNguoiDung, HanhDong, TenBang, MaBanGhi, DiaChiIP, ThoiGian
                FROM NhatKyAudit WHERE {dieuKien} ORDER BY ThoiGian DESC, MaAudit DESC
                OFFSET @ViTriBatDau ROWS FETCH NEXT @KichThuocTrang ROWS ONLY;";
            using var ketQua = await ketNoi.QueryMultipleAsync(sql,
                new { boLoc.MaNguoiDung, boLoc.TenBang, boLoc.HanhDong, boLoc.MaBanGhi,
                    TuNgay = boLoc.TuNgay?.Date, DenNgay = boLoc.DenNgay?.Date.AddDays(1),
                    ViTriBatDau = (long)(boLoc.Trang - 1) * boLoc.KichThuocTrang, boLoc.KichThuocTrang }, giaoDich);
            long tongSoBanGhi = await ketQua.ReadSingleAsync<long>();
            var danhSach = (await ketQua.ReadAsync<NhatKyAudit>()).ToList();
            giaoDich.Commit();
            return (danhSach, tongSoBanGhi);
        }

        public async Task<NhatKyAudit?> LayTheoMa(long maAudit)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QuerySingleOrDefaultAsync<NhatKyAudit>(@"SELECT MaAudit, MaNguoiDung, HanhDong,
                TenBang, MaBanGhi, DuLieuCu, DuLieuMoi, DiaChiIP, ThoiGian FROM NhatKyAudit WHERE MaAudit = @MaAudit",
                new { MaAudit = maAudit });
        }

    }
}
