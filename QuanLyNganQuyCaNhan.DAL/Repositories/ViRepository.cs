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
    public class ViRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;
        public ViRepository(DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }
        public async Task<IEnumerable<Vi>> LayDanhSach(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT MaVi,MaNguoiDung,TenVi,SoDu,TrangThai,NgayTao,NgayCapNhat,NgayXoa
                               FROM Vi
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.QueryAsync<Vi>(cauLenh,new{MaNguoiDung = maNguoiDung});
        }

        public async Task<int> ThemVi(Vi vi)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"INSERT INTO Vi(MaNguoiDung,TenVi,SoDu,TrangThai)
                               VALUES (@MaNguoiDung,@TenVi,0,1);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";
            return await ketNoi.ExecuteScalarAsync<int>(cauLenh,vi);
        }

        public async Task<bool> KiemTraTenViTonTai(int maNguoiDung,string tenVi)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1) FROM Vi
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND TenVi = @TenVi
                               AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh,new{MaNguoiDung = maNguoiDung,TenVi = tenVi});
            return soLuong > 0;
        }

        public async Task<bool> KiemTraNguoiDungTonTai(int maNguoiDung)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @" SELECT COUNT(1) FROM NguoiDung
                                WHERE MaNguoiDung = @MaNguoiDung
                                AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh,new{MaNguoiDung = maNguoiDung});
            return soLuong > 0;
        }
        public async Task<int> CapNhatVi(Vi vi)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @" UPDATE Vi
                                SET TenVi = @TenVi,
                                TrangThai = @TrangThai,
                                NgayCapNhat = GETDATE()
                                WHERE MaVi = @MaVi
                                AND MaNguoiDung = @MaNguoiDung
                                AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,vi);
        }

        public async Task<bool> KiemTraTenViTrungKhiCapNhat(int maNguoiDung,string tenVi,int maVi)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"SELECT COUNT(1) FROM Vi
                               WHERE MaNguoiDung = @MaNguoiDung
                               AND TenVi = @TenVi
                               AND MaVi <> @MaVi
                               AND NgayXoa IS NULL
            ";
            int soLuong = await ketNoi.ExecuteScalarAsync<int>(cauLenh,new{MaNguoiDung = maNguoiDung,TenVi = tenVi,MaVi = maVi});
            return soLuong > 0;
        }

        public async Task<int> XoaMemVi(int maVi)
        {
            using var ketNoi = ketNoiCSDL.TaoKetNoi();
            string cauLenh = @"UPDATE Vi
                               SET NgayXoa = GETDATE()
                               WHERE MaVi = @MaVi
                               AND NgayXoa IS NULL
            ";
            return await ketNoi.ExecuteAsync(cauLenh,new{MaVi = maVi});
        }
    }
}
