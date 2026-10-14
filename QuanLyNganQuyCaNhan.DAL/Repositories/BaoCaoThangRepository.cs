using Dapper;
using System.Data;
using System.Text.Json;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class BaoCaoThangRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;
        private const string CacCot = "MaBaoCao, MaNguoiDung, MaYeuCau, Thang, Nam, TrangThai, SoLanThu, NgayTao, NgayBatDau, NgayHoanThanh, LoiCuoi, CAST(COALESCE(DATALENGTH(FilePDF), 0) AS BIGINT) AS KichThuocFile";
        private const string NguoiDungHoatDong = "EXISTS (SELECT 1 FROM NguoiDung nd WHERE nd.MaNguoiDung = @MaNguoiDung AND nd.TrangThai = 1 AND nd.NgayXoa IS NULL)";

        public BaoCaoThangRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<BaoCaoThang> TaoYeuCau(int maNguoiDung, int thang, int nam, Guid maYeuCau, string? diaChiIP)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using var giaoDich = ketNoi.BeginTransaction();
            try
            {
                bool hopLe = await ketNoi.ExecuteScalarAsync<bool>(@"SELECT CASE WHEN EXISTS
                    (SELECT 1 FROM NguoiDung WITH (UPDLOCK, HOLDLOCK) WHERE MaNguoiDung = @MaNguoiDung
                     AND TrangThai = 1 AND NgayXoa IS NULL) THEN 1 ELSE 0 END", new { MaNguoiDung = maNguoiDung }, giaoDich);
                if (!hopLe) throw new ArgumentException("Người dùng không tồn tại hoặc đã bị khóa");
                var cu = await ketNoi.QuerySingleOrDefaultAsync<BaoCaoThang>($@"SELECT {CacCot} FROM BaoCaoThang WITH (UPDLOCK, HOLDLOCK)
                    WHERE MaNguoiDung = @MaNguoiDung AND MaYeuCau = @MaYeuCau",
                    new { MaNguoiDung = maNguoiDung, MaYeuCau = maYeuCau }, giaoDich);
                if (cu != null)
                {
                    if (cu.Thang != thang || cu.Nam != nam) throw new ArgumentException("Mã yêu cầu đã dùng cho tháng/năm khác");
                    giaoDich.Commit();
                    return cu;
                }
                int dangCho = await ketNoi.ExecuteScalarAsync<int>(@"SELECT COUNT(1) FROM BaoCaoThang WITH (UPDLOCK, HOLDLOCK)
                    WHERE MaNguoiDung = @MaNguoiDung AND TrangThai IN ('CHO_XU_LY', 'DANG_XU_LY')", new { MaNguoiDung = maNguoiDung }, giaoDich);
                if (dangCho >= 5) throw new ArgumentException("Tối đa 5 báo cáo đang chờ/xử lý; hãy chờ các yêu cầu trước hoàn tất");
                long ma = await ketNoi.ExecuteScalarAsync<long>(@"INSERT INTO BaoCaoThang (MaNguoiDung, MaYeuCau, Thang, Nam)
                    VALUES (@MaNguoiDung, @MaYeuCau, @Thang, @Nam); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);",
                    new { MaNguoiDung = maNguoiDung, MaYeuCau = maYeuCau, Thang = thang, Nam = nam }, giaoDich);
                await ketNoi.ExecuteAsync(@"INSERT INTO NhatKyAudit (MaNguoiDung, HanhDong, TenBang, MaBanGhi, DuLieuMoi, DiaChiIP)
                    VALUES (@MaNguoiDung, 'YEU_CAU_BAO_CAO', 'BaoCaoThang', @MaBanGhi, @DuLieuMoi, @DiaChiIP)",
                    new { MaNguoiDung = maNguoiDung, MaBanGhi = ma.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        DuLieuMoi = JsonSerializer.Serialize(new { Thang = thang, Nam = nam }), DiaChiIP = diaChiIP }, giaoDich);
                var moi = await ketNoi.QuerySingleAsync<BaoCaoThang>($"SELECT {CacCot} FROM BaoCaoThang WHERE MaBaoCao = @MaBaoCao", new { MaBaoCao = ma }, giaoDich);
                giaoDich.Commit();
                return moi;
            }
            catch { try { giaoDich.Rollback(); } catch { } throw; }
        }

        public async Task<BaoCaoThang?> LayTheoMa(long maBaoCao, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QuerySingleOrDefaultAsync<BaoCaoThang>($@"SELECT {CacCot} FROM BaoCaoThang
                WHERE MaBaoCao = @MaBaoCao AND MaNguoiDung = @MaNguoiDung AND {NguoiDungHoatDong}", new { MaBaoCao = maBaoCao, MaNguoiDung = maNguoiDung });
        }

        public async Task<(IEnumerable<BaoCaoThang> DanhSach, int TongSoBanGhi)> LayDanhSach(int maNguoiDung, int trang, int kichThuocTrang)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            using var ketQua = await ketNoi.QueryMultipleAsync($@"SELECT COUNT(1) FROM BaoCaoThang WHERE MaNguoiDung = @MaNguoiDung AND {NguoiDungHoatDong};
                SELECT {CacCot} FROM BaoCaoThang WHERE MaNguoiDung = @MaNguoiDung AND {NguoiDungHoatDong}
                ORDER BY MaBaoCao DESC OFFSET @ViTri ROWS FETCH NEXT @KichThuocTrang ROWS ONLY;",
                new { MaNguoiDung = maNguoiDung, ViTri = (long)(trang - 1) * kichThuocTrang, KichThuocTrang = kichThuocTrang });
            int tong = await ketQua.ReadSingleAsync<int>();
            return (await ketQua.ReadAsync<BaoCaoThang>(), tong);
        }

        public async Task<byte[]?> LayFile(long maBaoCao, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.ExecuteScalarAsync<byte[]?>($@"SELECT FilePDF FROM BaoCaoThang WHERE MaBaoCao = @MaBaoCao
                AND MaNguoiDung = @MaNguoiDung AND TrangThai = 'HOAN_THANH' AND {NguoiDungHoatDong}", new { MaBaoCao = maBaoCao, MaNguoiDung = maNguoiDung });
        }

        public async Task<BaoCaoThang?> NhanYeuCau(CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            return await ketNoi.QuerySingleOrDefaultAsync<BaoCaoThang>(new CommandDefinition(@"
                UPDATE BaoCaoThang SET TrangThai = 'LOI', NgayHoanThanh = SYSUTCDATETIME(),
                    LoiCuoi = N'Xử lý bị gián đoạn quá số lần cho phép; hãy tạo yêu cầu mới', MaLanXuLy = NULL, HetHanXuLy = NULL
                WHERE TrangThai = 'DANG_XU_LY' AND HetHanXuLy <= SYSUTCDATETIME() AND SoLanThu >= 3;
                ;WITH YeuCau AS
                (SELECT TOP (1) bc.* FROM BaoCaoThang bc WITH (UPDLOCK, ROWLOCK)
                    WHERE SoLanThu < 3 AND ((bc.TrangThai = 'CHO_XU_LY' AND bc.NgayThuLai <= SYSUTCDATETIME())
                        OR (bc.TrangThai = 'DANG_XU_LY' AND bc.HetHanXuLy <= SYSUTCDATETIME()))
                      AND EXISTS (SELECT 1 FROM NguoiDung nd WHERE nd.MaNguoiDung = bc.MaNguoiDung AND nd.TrangThai = 1 AND nd.NgayXoa IS NULL)
                    ORDER BY bc.MaBaoCao)
                UPDATE YeuCau SET TrangThai = 'DANG_XU_LY', SoLanThu = SoLanThu + 1,
                    NgayBatDau = SYSUTCDATETIME(), HetHanXuLy = DATEADD(MINUTE, 10, SYSUTCDATETIME()), MaLanXuLy = NEWID()
                OUTPUT inserted.MaBaoCao, inserted.MaNguoiDung, inserted.MaYeuCau, inserted.Thang, inserted.Nam,
                    inserted.TrangThai, inserted.SoLanThu, inserted.NgayTao, inserted.NgayBatDau, inserted.MaLanXuLy;",
                cancellationToken: cancellationToken));
        }

        public async Task<DuLieuBaoCaoThang> LayDuLieu(BaoCaoThang baoCao, CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync(cancellationToken);
            using var giaoDich = ketNoi.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                var thamSo = new { baoCao.MaNguoiDung, TuNgay = new DateTime(baoCao.Nam, baoCao.Thang, 1),
                    DenNgay = new DateTime(baoCao.Nam, baoCao.Thang, 1).AddMonths(1) };
                string? hoTen = await ketNoi.ExecuteScalarAsync<string?>(new CommandDefinition(@"SELECT HoTen FROM NguoiDung
                    WHERE MaNguoiDung = @MaNguoiDung AND TrangThai = 1 AND NgayXoa IS NULL", thamSo, giaoDich, cancellationToken: cancellationToken));
                if (hoTen == null) throw new ArgumentException("Người dùng không còn hoạt động");
                var danhSach = (await ketNoi.QueryAsync<GiaoDich>(new CommandDefinition(@"SELECT TOP (10001) gd.MaGiaoDich, gd.MaVi, gd.MaDanhMuc,
                        gd.LoaiGiaoDich, gd.SoTien, gd.NgayGiaoDich, gd.NoiDung, dm.TenDanhMuc, v.TenVi
                    FROM GiaoDich gd LEFT JOIN DanhMuc dm ON dm.MaDanhMuc = gd.MaDanhMuc AND (dm.MaNguoiDung = @MaNguoiDung OR dm.MaNguoiDung IS NULL)
                        LEFT JOIN Vi v ON v.MaVi = gd.MaVi AND v.MaNguoiDung = @MaNguoiDung
                    WHERE gd.MaNguoiDung = @MaNguoiDung AND gd.NgayXoa IS NULL AND gd.NgayGiaoDich >= @TuNgay AND gd.NgayGiaoDich < @DenNgay
                    ORDER BY gd.NgayGiaoDich, gd.MaGiaoDich", thamSo, giaoDich, cancellationToken: cancellationToken))).ToList();
                if (danhSach.Count > 10000) throw new ArgumentException("Tháng có hơn 10.000 giao dịch; chưa hỗ trợ báo cáo chi tiết lớn hơn giới hạn này");
                giaoDich.Commit();
                return new DuLieuBaoCaoThang { HoTen = hoTen, Thang = baoCao.Thang, Nam = baoCao.Nam, NgayLayDuLieu = DateTime.UtcNow, DanhSach = danhSach };
            }
            catch { try { giaoDich.Rollback(); } catch { } throw; }
        }

        public async Task HoanThanh(BaoCaoThang baoCao, byte[] filePDF, CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.ExecuteAsync(new CommandDefinition(@"UPDATE BaoCaoThang SET FilePDF = @FilePDF, TrangThai = 'HOAN_THANH',
                NgayHoanThanh = SYSUTCDATETIME(), LoiCuoi = NULL, HetHanXuLy = NULL, MaLanXuLy = NULL
                WHERE MaBaoCao = @MaBaoCao AND TrangThai = 'DANG_XU_LY' AND MaLanXuLy = @MaLanXuLy",
                new { baoCao.MaBaoCao, baoCao.MaLanXuLy, FilePDF = filePDF }, cancellationToken: cancellationToken));
        }

        public async Task GhiLoi(BaoCaoThang baoCao, string loi, bool loiDuLieu, CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.ExecuteAsync(new CommandDefinition(@"UPDATE BaoCaoThang SET TrangThai = @TrangThai, LoiCuoi = @LoiCuoi,
                NgayThuLai = DATEADD(MINUTE, 1, SYSUTCDATETIME()), HetHanXuLy = NULL, MaLanXuLy = NULL,
                NgayHoanThanh = CASE WHEN @TrangThai = 'LOI' THEN SYSUTCDATETIME() ELSE NULL END
                WHERE MaBaoCao = @MaBaoCao AND TrangThai = 'DANG_XU_LY' AND MaLanXuLy = @MaLanXuLy",
                new { baoCao.MaBaoCao, baoCao.MaLanXuLy, LoiCuoi = loi,
                    TrangThai = loiDuLieu || baoCao.SoLanThu >= 3 ? "LOI" : "CHO_XU_LY" }, cancellationToken: cancellationToken));
        }
    }
}
