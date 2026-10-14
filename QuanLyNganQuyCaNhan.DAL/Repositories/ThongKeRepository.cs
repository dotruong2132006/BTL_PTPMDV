using Dapper;
using System.Data;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class ThongKeRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;
        public ThongKeRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        private const string KiemTraNguoiDung = @"IF NOT EXISTS (SELECT 1 FROM NguoiDung
            WHERE MaNguoiDung = @MaNguoiDung AND TrangThai = 1 AND NgayXoa IS NULL)
            THROW 50001, N'Người dùng không còn hoạt động', 1;";

        public async Task<TongQuanTaiChinh> LayTongQuan(int maNguoiDung, int thang, int nam, CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync(cancellationToken);
            using var giaoDich = ketNoi.BeginTransaction(IsolationLevel.Serializable);
            var tuNgay = new DateTime(nam, thang, 1);
            string sql = KiemTraNguoiDung + @"
                SELECT MaVi, MaNguoiDung, TenVi, SoDu, MoTa, TrangThai, NgayTao, NgayCapNhat
                FROM Vi WHERE MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL ORDER BY MaVi;
                SELECT COALESCE(SUM(CASE WHEN LoaiGiaoDich = 'THU' THEN SoTien ELSE 0 END), 0) AS TongThu,
                       COALESCE(SUM(CASE WHEN LoaiGiaoDich = 'CHI' THEN SoTien ELSE 0 END), 0) AS TongChi,
                       COUNT_BIG(*) AS SoGiaoDich
                FROM GiaoDich WHERE MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL
                    AND NgayGiaoDich >= @TuNgay AND NgayGiaoDich < @DenNgay;";
            using var ketQua = await ketNoi.QueryMultipleAsync(new CommandDefinition(sql,
                new { MaNguoiDung = maNguoiDung, TuNgay = tuNgay, DenNgay = tuNgay.AddMonths(1) }, giaoDich,
                cancellationToken: cancellationToken));
            // Đọc ví trước giao dịch theo thứ tự cập nhật nghiệp vụ để giảm nguy cơ deadlock.
            var danhSachVi = (await ketQua.ReadAsync<Vi>()).ToList();
            var tongQuan = await ketQua.ReadSingleAsync<TongQuanTaiChinh>();
            tongQuan.Thang = thang;
            tongQuan.Nam = nam;
            tongQuan.DanhSachVi = danhSachVi;
            giaoDich.Commit();
            return tongQuan;
        }

        public async Task<IEnumerable<ThongKeThang>> LayTheoThang(int maNguoiDung, int nam, int? maVi, CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string sql = KiemTraNguoiDung + @"
                SELECT MONTH(NgayGiaoDich) AS Thang, @Nam AS Nam,
                       SUM(CASE WHEN LoaiGiaoDich = 'THU' THEN SoTien ELSE 0 END) AS TongThu,
                       SUM(CASE WHEN LoaiGiaoDich = 'CHI' THEN SoTien ELSE 0 END) AS TongChi,
                       COUNT_BIG(*) AS SoGiaoDich
                FROM GiaoDich WHERE MaNguoiDung = @MaNguoiDung AND NgayXoa IS NULL
                    AND NgayGiaoDich >= @TuNgay AND NgayGiaoDich < @DenNgay
                    AND (@MaVi IS NULL OR MaVi = @MaVi)
                GROUP BY MONTH(NgayGiaoDich) ORDER BY Thang;";
            return await ketNoi.QueryAsync<ThongKeThang>(new CommandDefinition(sql,
                new { MaNguoiDung = maNguoiDung, Nam = nam, MaVi = maVi,
                    TuNgay = new DateTime(nam, 1, 1), DenNgay = new DateTime(nam + 1, 1, 1) }, cancellationToken: cancellationToken));
        }

        public async Task<IEnumerable<ThongKeDanhMuc>> LayTheoDanhMuc(int maNguoiDung, int thang, int nam, int? maVi, CancellationToken cancellationToken)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            var tuNgay = new DateTime(nam, thang, 1);
            string sql = KiemTraNguoiDung + @"
                SELECT gd.MaDanhMuc, COALESCE(dm.TenDanhMuc, N'Không còn khả dụng') AS TenDanhMuc,
                       gd.LoaiGiaoDich, SUM(gd.SoTien) AS TongTien, COUNT_BIG(*) AS SoGiaoDich
                FROM GiaoDich gd LEFT JOIN DanhMuc dm ON dm.MaDanhMuc = gd.MaDanhMuc
                    AND (dm.MaNguoiDung = @MaNguoiDung OR dm.MaNguoiDung IS NULL)
                WHERE gd.MaNguoiDung = @MaNguoiDung AND gd.NgayXoa IS NULL
                    AND gd.NgayGiaoDich >= @TuNgay AND gd.NgayGiaoDich < @DenNgay
                    AND (@MaVi IS NULL OR gd.MaVi = @MaVi)
                GROUP BY gd.MaDanhMuc, dm.TenDanhMuc, gd.LoaiGiaoDich
                ORDER BY gd.LoaiGiaoDich, TongTien DESC, gd.MaDanhMuc;";
            return await ketNoi.QueryAsync<ThongKeDanhMuc>(new CommandDefinition(sql,
                new { MaNguoiDung = maNguoiDung, MaVi = maVi, TuNgay = tuNgay, DenNgay = tuNgay.AddMonths(1) }, cancellationToken: cancellationToken));
        }
    }
}
