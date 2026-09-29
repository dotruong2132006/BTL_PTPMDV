using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Models;


namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class NguoiDungRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        public NguoiDungRepository(
            DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }
        public async Task<IEnumerable<NguoiDung>> LayDanhSach()
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();

            string cauLenh = @"SELECT MaNguoiDung,TenDangNhap,Email,MatKhau,HoTen,SoDienThoai,DaBat2FA,MaBiMat2FA,TrangThai,NgayTao,NgayCapNhat,NgayXoa
                FROM NguoiDung
                WHERE NgayXoa IS NULL
                ORDER BY MaNguoiDung DESC
            ";
            return await ketNoi.QueryAsync<NguoiDung>(cauLenh);
        }
        public async Task<int> ThemNguoiDung(NguoiDung nguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();

            string cauLenh = @"INSERT INTO NguoiDung (TenDangNhap,Email,MatKhau,HoTen,SoDienThoai,DaBat2FA,MaBiMat2FA,TrangThai)
                VALUES(@TenDangNhap,@Email,@MatKhau,@HoTen,@SoDienThoai,0,'',1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";
            return await ketNoi.ExecuteScalarAsync<int>(cauLenh,nguoiDung);
        }
        public async Task<bool> KiemTraTenDangNhapTonTai(string tenDangNhap)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1)FROM NguoiDung
                WHERE TenDangNhap = @TenDangNhap
                AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh,new { TenDangNhap = tenDangNhap });
            return soLuong > 0;
        }
        public async Task<bool> KiemTraEmailTonTai(string email)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();

            string cauLenh = @"SELECT COUNT(1)FROM NguoiDung
                WHERE Email = @Email
                AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh,new { Email = email });
            return soLuong > 0;
        }
        public async Task<int> CapNhatNguoiDung(NguoiDung nguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE NguoiDung SET
                               Email = @Email,
                               HoTen = @HoTen,
                               SoDienThoai = @SoDienThoai,
                               TrangThai = @TrangThai,
                               NgayCapNhat = GETDATE()
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,nguoiDung);
        }
        public async Task<bool> KiemTraEmailTrungKhiCapNhat(string email,int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1) FROM NguoiDung
                               WHERE Email = @Email
                               AND MaNguoiDung <> @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>( cauLenh, new{Email = email,MaNguoiDung = maNguoiDung});
            return soLuong > 0;
        }
        public async Task<int> XoaMemNguoiDung(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE NguoiDung
                               SET NgayXoa = GETDATE()
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaNguoiDung = maNguoiDung});
        }
        public async Task<bool> KiemTraNguoiDungTonTai(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1) FROM NguoiDung
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh,new{MaNguoiDung = maNguoiDung});
            return soLuong > 0;
        }
        public async Task<NguoiDung?> LayTheoTenDangNhap(string tenDangNhap)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT MaNguoiDung,TenDangNhap,Email,MatKhau,HoTen,SoDienThoai,DaBat2FA,MaBiMat2FA,TrangThai,NgayTao,NgayCapNhat,NgayXoa
                               FROM NguoiDung
                               WHERE TenDangNhap = @TenDangNhap
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.QueryFirstOrDefaultAsync<NguoiDung>(cauLenh,new{TenDangNhap = tenDangNhap});
        }
        public async Task<string?> LayVaiTro(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT TOP 1 vt.TenVaiTro
                               FROM NguoiDung_VaiTro ndvt
                               INNER JOIN VaiTro vt
                               ON ndvt.MaVaiTro = vt.MaVaiTro
                               WHERE ndvt.MaNguoiDung = @MaNguoiDung
            ";
            return await ketNoi.QueryFirstOrDefaultAsync<string>(cauLenh,new{MaNguoiDung = maNguoiDung});
        }
        public async Task<NguoiDung?> LayTheoMaNguoiDung(int maNguoiDung)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT MaNguoiDung,TenDangNhap,Email,MatKhau,HoTen,SoDienThoai,DaBat2FA,MaBiMat2FA,TrangThai,NgayTao,NgayCapNhat,NgayXoa
                               FROM NguoiDung
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.QueryFirstOrDefaultAsync<NguoiDung>(cauLenh,new{MaNguoiDung = maNguoiDung});
        }
        public async Task<int> Bat2FA(int maNguoiDung,string maBiMat2FA)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE NguoiDung
                               SET DaBat2FA = 1, MaBiMat2FA = @MaBiMat2FA, NgayCapNhat = GETDATE()
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaNguoiDung = maNguoiDung,MaBiMat2FA = maBiMat2FA});
        }
        public async Task<int> Tat2FA(int maNguoiDung)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE NguoiDung
                               SET DaBat2FA = 0,MaBiMat2FA = NULL,NgayCapNhat = GETDATE()
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaNguoiDung = maNguoiDung});
        }
        public async Task<int> CapNhatMatKhau(int maNguoiDung,string matKhauMoi)
        {
            using var ketNoi =ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE NguoiDung
                               SET MatKhau = @MatKhauMoi,NgayCapNhat = GETDATE()
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaNguoiDung = maNguoiDung,MatKhauMoi = matKhauMoi});
        }
    }
}
