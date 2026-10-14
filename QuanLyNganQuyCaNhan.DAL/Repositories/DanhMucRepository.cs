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
    public partial class DanhMucRepository
    {
        private readonly DapperConnectionFactory ketNoi;

        public DanhMucRepository(DapperConnectionFactory ketNoi)
        {
            this.ketNoi = ketNoi;
        }
        public async Task<IEnumerable<DanhMuc>> LayDanhSach(int maNguoiDung)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            string sql = @"SELECT MaDanhMuc,MaNguoiDung,TenDanhMuc,LoaiDanhMuc,MoTa,NgayTao,NgayXoa
                           FROM DanhMuc
                           WHERE (MaNguoiDung = @MaNguoiDung OR MaNguoiDung IS NULL)
                           AND NgayXoa IS NULL
                           ORDER BY MaDanhMuc DESC";
            return await ketNoiCSDL.QueryAsync<DanhMuc>(sql,new{MaNguoiDung = maNguoiDung});
        }
        public async Task<bool> KiemTraNguoiDungTonTai(int maNguoiDung)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();

            string sql = @"SELECT COUNT(1) FROM NguoiDung
                           WHERE MaNguoiDung = @MaNguoiDung
                           AND TrangThai = 1
                           AND NgayXoa IS NULL";
            int soLuong = await ketNoiCSDL.ExecuteScalarAsync<int>(sql,new{MaNguoiDung = maNguoiDung});
            return soLuong > 0;
        }
        public async Task<bool> KiemTraTenDanhMucTonTai(int maNguoiDung,string tenDanhMuc,int maDanhMuc = 0)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            string sql = @"SELECT COUNT(1) FROM DanhMuc
                           WHERE MaNguoiDung = @MaNguoiDung
                           AND TenDanhMuc = @TenDanhMuc
                           AND NgayXoa IS NULL
                           AND MaDanhMuc <> @MaDanhMuc";
            int soLuong = await ketNoiCSDL.ExecuteScalarAsync<int>(sql,new{MaNguoiDung = maNguoiDung,TenDanhMuc = tenDanhMuc,MaDanhMuc = maDanhMuc});
            return soLuong > 0;
        }
        public async Task<int> ThemDanhMuc(DanhMuc danhMuc)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            string sql = @"INSERT INTO DanhMuc(MaNguoiDung,TenDanhMuc,LoaiDanhMuc,MoTa)
                           VALUES(@MaNguoiDung,@TenDanhMuc,@LoaiDanhMuc,@MoTa);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await ketNoiCSDL.ExecuteScalarAsync<int>(sql,danhMuc);
        }
        public async Task<int> CapNhatDanhMuc(DanhMuc danhMuc)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            string sql = @"UPDATE DanhMuc SET TenDanhMuc = @TenDanhMuc,LoaiDanhMuc = @LoaiDanhMuc,MoTa = @MoTa
                           WHERE MaDanhMuc = @MaDanhMuc
                           AND MaNguoiDung = @MaNguoiDung
                           AND NgayXoa IS NULL";
            return await ketNoiCSDL.ExecuteAsync(sql,danhMuc);
        }

        public async Task<int> XoaMemDanhMuc(int maDanhMuc, int maNguoiDung)
        {
            using var ketNoiCSDL = ketNoi.TaoKetNoi();
            string sql = @"UPDATE DanhMuc
                           SET NgayXoa = GETDATE()
                           WHERE MaDanhMuc = @MaDanhMuc
                           AND MaNguoiDung = @MaNguoiDung
                           AND NgayXoa IS NULL";
            return await ketNoiCSDL.ExecuteAsync(sql, new { MaDanhMuc = maDanhMuc, MaNguoiDung = maNguoiDung });
        }
    }
}

