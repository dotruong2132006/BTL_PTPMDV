using Dapper;
using System.Data;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public partial class GiaoDichRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        public GiaoDichRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }

        public async Task<(IEnumerable<GiaoDich> DanhSach, int TongSoBanGhi)> LayDanhSach(
            int maNguoiDung,
            DateTime? tuNgay,
            DateTime? denNgay,
            int? maDanhMuc,
            int trang,
            int kichThuocTrang,
            string sapXepTheo,
            string chieuSapXep)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();

            var cotSapXepHopLe = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["NgayGiaoDich"] = "gd.NgayGiaoDich",
                ["SoTien"] = "gd.SoTien",
                ["TenDanhMuc"] = "dm.TenDanhMuc",
                ["TenVi"] = "v.TenVi",
                ["MaGiaoDich"] = "gd.MaGiaoDich"
            };

            if (!cotSapXepHopLe.TryGetValue(sapXepTheo, out string? cotSapXep))
            {
                throw new ArgumentException("Trường sắp xếp không hợp lệ");
            }

            string chieu = chieuSapXep.Equals("ASC", StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : chieuSapXep.Equals("DESC", StringComparison.OrdinalIgnoreCase)
                    ? "DESC"
                    : throw new ArgumentException("Chiều sắp xếp chỉ được là ASC hoặc DESC");

            string dieuKien = @"gd.MaNguoiDung = @MaNguoiDung
                AND gd.NgayXoa IS NULL
                AND (@TuNgay IS NULL OR gd.NgayGiaoDich >= @TuNgay)
                AND (@DenNgay IS NULL OR gd.NgayGiaoDich < DATEADD(DAY, 1, CONVERT(date, @DenNgay)))
                AND (@MaDanhMuc IS NULL OR gd.MaDanhMuc = @MaDanhMuc)";

            string cauLenh = $@"SELECT COUNT(1)
                               FROM GiaoDich gd
                               WHERE {dieuKien};

                               SELECT gd.MaGiaoDich, gd.MaNguoiDung, gd.MaVi, gd.MaDanhMuc,
                                      gd.LoaiGiaoDich, gd.SoTien, gd.NoiDung, gd.NgayGiaoDich,
                                      gd.PhienBan, gd.NgayTao, gd.NgayCapNhat, gd.NgayXoa,
                                      dm.TenDanhMuc, v.TenVi
                               FROM GiaoDich gd
                               INNER JOIN DanhMuc dm ON dm.MaDanhMuc = gd.MaDanhMuc
                                   AND (dm.MaNguoiDung = gd.MaNguoiDung OR dm.MaNguoiDung IS NULL)
                               INNER JOIN Vi v ON v.MaVi = gd.MaVi
                                   AND v.MaNguoiDung = gd.MaNguoiDung
                               WHERE {dieuKien}
                               ORDER BY {cotSapXep} {chieu}, gd.MaGiaoDich DESC
                               OFFSET @ViTriBatDau ROWS FETCH NEXT @KichThuocTrang ROWS ONLY;";

            var thamSo = new
            {
                MaNguoiDung = maNguoiDung,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                MaDanhMuc = maDanhMuc,
                ViTriBatDau = (long)(trang - 1) * kichThuocTrang,
                KichThuocTrang = kichThuocTrang
            };

            using var ketQua = await ketNoi.QueryMultipleAsync(cauLenh, thamSo);
            int tongSoBanGhi = await ketQua.ReadSingleAsync<int>();
            var danhSach = await ketQua.ReadAsync<GiaoDich>();
            return (danhSach, tongSoBanGhi);
        }

        public async Task<GiaoDich?> LayTheoMa(long maGiaoDich, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT gd.MaGiaoDich, gd.MaNguoiDung, gd.MaVi, gd.MaDanhMuc,
                                      gd.LoaiGiaoDich, gd.SoTien, gd.NoiDung, gd.NgayGiaoDich,
                                      gd.PhienBan, gd.NgayTao, gd.NgayCapNhat, gd.NgayXoa,
                                      dm.TenDanhMuc, v.TenVi
                               FROM GiaoDich gd
                               INNER JOIN DanhMuc dm ON dm.MaDanhMuc = gd.MaDanhMuc
                                   AND (dm.MaNguoiDung = gd.MaNguoiDung OR dm.MaNguoiDung IS NULL)
                               INNER JOIN Vi v ON v.MaVi = gd.MaVi
                                   AND v.MaNguoiDung = gd.MaNguoiDung
                               WHERE gd.MaGiaoDich = @MaGiaoDich
                               AND gd.MaNguoiDung = @MaNguoiDung
                               AND gd.NgayXoa IS NULL";
            return await ketNoi.QueryFirstOrDefaultAsync<GiaoDich>(cauLenh,new { MaGiaoDich = maGiaoDich, MaNguoiDung = maNguoiDung });
        }

        public async Task<bool> KiemTraNguoiDungTonTai(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1) FROM NguoiDung
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND TrangThai = 1
                               AND NgayXoa IS NULL";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh, new { MaNguoiDung = maNguoiDung });
            return soLuong > 0;
        }

        public async Task<bool> KiemTraViVaDanhMucHopLe(
            int maNguoiDung,
            int maVi,
            int maDanhMuc,
            string loaiGiaoDich)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1)
                               FROM Vi v
                               INNER JOIN DanhMuc dm ON (dm.MaNguoiDung = v.MaNguoiDung OR dm.MaNguoiDung IS NULL)
                               WHERE v.MaVi = @MaVi
                               AND v.MaNguoiDung = @MaNguoiDung
                               AND v.TrangThai = 1
                               AND v.NgayXoa IS NULL
                               AND dm.MaDanhMuc = @MaDanhMuc
                               AND (dm.MaNguoiDung = @MaNguoiDung OR dm.MaNguoiDung IS NULL)
                               AND dm.LoaiDanhMuc = @LoaiGiaoDich
                               AND dm.NgayXoa IS NULL";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh, new
            {
                MaNguoiDung = maNguoiDung,
                MaVi = maVi,
                MaDanhMuc = maDanhMuc,
                LoaiGiaoDich = loaiGiaoDich
            });
            return soLuong > 0;
        }

        public async Task<long> ThemGiaoDich(GiaoDich giaoDich)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using IDbTransaction giaoDichCSDL = ketNoi.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                if (!await KiemTraGiaoDichHopLeTrongGiaoDich(
                    ketNoi, giaoDichCSDL, giaoDich, new[] { giaoDich.MaVi }))
                    throw new ArgumentException("Ví hoặc danh mục không hợp lệ, không hoạt động, hoặc không khớp loại giao dịch");

                await CapNhatSoDuVi(ketNoi, giaoDichCSDL, giaoDich.MaNguoiDung, giaoDich.MaVi,
                    TinhBienDong(giaoDich.LoaiGiaoDich, giaoDich.SoTien));

                string cauLenh = @"INSERT INTO GiaoDich
                                       (MaNguoiDung, MaVi, MaDanhMuc, LoaiGiaoDich, SoTien, NoiDung, NgayGiaoDich)
                                   VALUES
                                       (@MaNguoiDung, @MaVi, @MaDanhMuc, @LoaiGiaoDich, @SoTien, @NoiDung, @NgayGiaoDich);
                                   SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                long maGiaoDich = await ketNoi.ExecuteScalarAsync<long>(cauLenh, giaoDich, giaoDichCSDL);

                giaoDichCSDL.Commit();
                return maGiaoDich;
            }
            catch
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw;
            }
        }

        public async Task<int> CapNhatGiaoDich(GiaoDich giaoDich)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using IDbTransaction giaoDichCSDL = ketNoi.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                string cauLenhLayGiaoDich = @"SELECT MaGiaoDich, MaNguoiDung, MaVi, MaDanhMuc,
                                                     LoaiGiaoDich, SoTien, PhienBan
                                              FROM GiaoDich WITH (UPDLOCK, HOLDLOCK)
                                              WHERE MaGiaoDich = @MaGiaoDich
                                                AND MaNguoiDung = @MaNguoiDung
                                                AND NgayXoa IS NULL";
                GiaoDich? giaoDichCu = await ketNoi.QueryFirstOrDefaultAsync<GiaoDich>(
                    cauLenhLayGiaoDich,
                    new { giaoDich.MaGiaoDich, giaoDich.MaNguoiDung },
                    giaoDichCSDL);

                if (giaoDichCu == null || giaoDichCu.PhienBan != giaoDich.PhienBan)
                {
                    giaoDichCSDL.Rollback();
                    return 0;
                }

                if (!await KiemTraGiaoDichHopLeTrongGiaoDich(
                    ketNoi, giaoDichCSDL, giaoDich, new[] { giaoDichCu.MaVi, giaoDich.MaVi }))
                    throw new ArgumentException("Ví hoặc danh mục không hợp lệ, không hoạt động, hoặc không khớp loại giao dịch");

                var bienDongSoDuTheoVi = new Dictionary<int, decimal>();
                ThemBienDong(bienDongSoDuTheoVi, giaoDichCu.MaVi,
                    -TinhBienDong(giaoDichCu.LoaiGiaoDich, giaoDichCu.SoTien));
                ThemBienDong(bienDongSoDuTheoVi, giaoDich.MaVi,
                    TinhBienDong(giaoDich.LoaiGiaoDich, giaoDich.SoTien));

                foreach (var bienDong in bienDongSoDuTheoVi.OrderBy(x => x.Key))
                {
                    await CapNhatSoDuVi(ketNoi, giaoDichCSDL, giaoDich.MaNguoiDung,
                        bienDong.Key, bienDong.Value);
                }

                string cauLenhCapNhat = @"UPDATE GiaoDich
                                         SET MaVi = @MaVi,
                                             MaDanhMuc = @MaDanhMuc,
                                             LoaiGiaoDich = @LoaiGiaoDich,
                                             SoTien = @SoTien,
                                             NoiDung = @NoiDung,
                                             NgayGiaoDich = @NgayGiaoDich,
                                             PhienBan = PhienBan + 1,
                                             NgayCapNhat = SYSDATETIME()
                                         WHERE MaGiaoDich = @MaGiaoDich
                                           AND MaNguoiDung = @MaNguoiDung
                                           AND PhienBan = @PhienBan
                                           AND NgayXoa IS NULL";
                int soDong = await ketNoi.ExecuteAsync(cauLenhCapNhat, giaoDich, giaoDichCSDL);
                if (soDong != 1)
                {
                    giaoDichCSDL.Rollback();
                    return 0;
                }

                giaoDichCSDL.Commit();
                return soDong;
            }
            catch
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw;
            }
        }

        public async Task<int> XoaMemGiaoDich(long maGiaoDich, int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            await ketNoi.OpenAsync();
            using IDbTransaction giaoDichCSDL = ketNoi.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                string cauLenhLayGiaoDich = @"SELECT MaGiaoDich, MaNguoiDung, MaVi, LoaiGiaoDich, SoTien, PhienBan
                                              FROM GiaoDich WITH (UPDLOCK, HOLDLOCK)
                                              WHERE MaGiaoDich = @MaGiaoDich
                                                AND MaNguoiDung = @MaNguoiDung
                                                AND NgayXoa IS NULL";
                GiaoDich? giaoDichCu = await ketNoi.QueryFirstOrDefaultAsync<GiaoDich>(
                    cauLenhLayGiaoDich,
                    new { MaGiaoDich = maGiaoDich, MaNguoiDung = maNguoiDung },
                    giaoDichCSDL);

                if (giaoDichCu == null)
                {
                    giaoDichCSDL.Rollback();
                    return 0;
                }

                await CapNhatSoDuVi(ketNoi, giaoDichCSDL, maNguoiDung, giaoDichCu.MaVi,-TinhBienDong(giaoDichCu.LoaiGiaoDich, giaoDichCu.SoTien));

                string cauLenhXoaMem = @"UPDATE GiaoDich
                                        SET NgayXoa = SYSDATETIME(),
                                            NgayCapNhat = SYSDATETIME(),
                                            PhienBan = PhienBan + 1
                                        WHERE MaGiaoDich = @MaGiaoDich
                                          AND MaNguoiDung = @MaNguoiDung
                                          AND PhienBan = @PhienBan
                                          AND NgayXoa IS NULL";
                int soDong = await ketNoi.ExecuteAsync(cauLenhXoaMem, new
                {
                    MaGiaoDich = giaoDichCu.MaGiaoDich,
                    MaNguoiDung = maNguoiDung,
                    giaoDichCu.PhienBan
                }, giaoDichCSDL);

                if (soDong != 1)
                {
                    giaoDichCSDL.Rollback();
                    return 0;
                }

                giaoDichCSDL.Commit();
                return soDong;
            }
            catch
            {
                try { giaoDichCSDL.Rollback(); } catch { }
                throw;
            }
        }

        private static decimal TinhBienDong(string loaiGiaoDich, decimal soTien)
        {
            return loaiGiaoDich == "THU" ? soTien : -soTien;
        }

        private static void ThemBienDong(Dictionary<int, decimal> cacBienDong, int maVi, decimal soTien)
        {
            cacBienDong[maVi] = cacBienDong.GetValueOrDefault(maVi) + soTien;
        }

        private static async Task<bool> KiemTraGiaoDichHopLeTrongGiaoDich(
            System.Data.IDbConnection ketNoi,
            IDbTransaction giaoDichCSDL,
            GiaoDich giaoDich,
            IEnumerable<int> cacMaViCanKhoa)
        {
            string cauLenhKiemTraNguoiDung = @"SELECT COUNT(1)
                                               FROM NguoiDung WITH (HOLDLOCK)
                                               WHERE MaNguoiDung = @MaNguoiDung
                                                 AND TrangThai = 1
                                                 AND NgayXoa IS NULL";
            int nguoiDungHopLe = await ketNoi.ExecuteScalarAsync<int>(cauLenhKiemTraNguoiDung,
                new { giaoDich.MaNguoiDung }, giaoDichCSDL);
            if (nguoiDungHopLe != 1)
                return false;

            foreach (int maVi in cacMaViCanKhoa.Distinct().OrderBy(x => x))
            {
                string cauLenhKhoaVi = @"SELECT MaVi
                                         FROM Vi WITH (UPDLOCK, HOLDLOCK)
                                         WHERE MaVi = @MaVi
                                           AND MaNguoiDung = @MaNguoiDung";
                int? viTonTai = await ketNoi.QueryFirstOrDefaultAsync<int?>(cauLenhKhoaVi,
                    new { MaVi = maVi, giaoDich.MaNguoiDung }, giaoDichCSDL);
                if (!viTonTai.HasValue)
                    return false;
            }

            string cauLenhKiemTraVi = @"SELECT COUNT(1)
                                        FROM Vi
                                        WHERE MaVi = @MaVi
                                          AND MaNguoiDung = @MaNguoiDung
                                          AND TrangThai = 1
                                          AND NgayXoa IS NULL";
            int viDichHopLe = await ketNoi.ExecuteScalarAsync<int>(cauLenhKiemTraVi,
                new { giaoDich.MaVi, giaoDich.MaNguoiDung }, giaoDichCSDL);
            if (viDichHopLe != 1)
                return false;

            string cauLenhKiemTraDanhMuc = @"SELECT COUNT(1)
                                             FROM DanhMuc WITH (HOLDLOCK)
                                             WHERE MaDanhMuc = @MaDanhMuc
                                               AND (MaNguoiDung = @MaNguoiDung OR MaNguoiDung IS NULL)
                                               AND LoaiDanhMuc = @LoaiGiaoDich
                                               AND NgayXoa IS NULL";
            int danhMucHopLe = await ketNoi.ExecuteScalarAsync<int>(cauLenhKiemTraDanhMuc,
                new
                {
                    giaoDich.MaDanhMuc,
                    giaoDich.MaNguoiDung,
                    giaoDich.LoaiGiaoDich
                }, giaoDichCSDL);

            return danhMucHopLe == 1;
        }

        private static async Task CapNhatSoDuVi(
            System.Data.IDbConnection ketNoi,
            IDbTransaction giaoDichCSDL,
            int maNguoiDung,
            int maVi,
            decimal bienDong)
        {
            if (bienDong == 0)
                return;

            string cauLenh = @"UPDATE Vi
                               SET SoDu = SoDu + @BienDong,
                                   NgayCapNhat = SYSDATETIME()
                               WHERE MaVi = @MaVi
                                 AND MaNguoiDung = @MaNguoiDung";

            int soDong = await ketNoi.ExecuteAsync(cauLenh, new
            {
                MaNguoiDung = maNguoiDung,
                MaVi = maVi,
                BienDong = bienDong
            }, giaoDichCSDL);

            if (soDong == 0)
            {
                throw new ArgumentException("Không tìm thấy ví của người dùng");
            }
        }
    }
}
