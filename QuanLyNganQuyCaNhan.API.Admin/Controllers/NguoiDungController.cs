using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.API.Admin.DTOs;
using QuanLyNganQuyCaNhan.DAL.Models;
using Microsoft.AspNetCore.Authorization;


namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class NguoiDungController : ControllerBase
    {
        private readonly INguoiDungService nguoiDungService;
        public NguoiDungController(INguoiDungService nguoiDungService)
        {
            this.nguoiDungService = nguoiDungService;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach()
        {
            var danhSach = await nguoiDungService.LayDanhSach();
            var ketQua = danhSach.Select(x => new NguoiDungDTO
            {
                MaNguoiDung = x.MaNguoiDung,
                TenDangNhap = x.TenDangNhap,
                Email = x.Email,
                HoTen = x.HoTen,
                SoDienThoai = x.SoDienThoai,
                DaBat2FA = x.DaBat2FA,
                TrangThai = x.TrangThai,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat,
                NgayXoa = x.NgayXoa
            });
            return Ok(ketQua);
        }

        [HttpPost]
        public async Task<IActionResult> ThemNguoiDung(ThemNguoiDungDTO duLieu)
        {
            try
            {
                NguoiDung nguoiDung = new NguoiDung
                {
                    TenDangNhap = duLieu.TenDangNhap,
                    Email = duLieu.Email,
                    MatKhau = duLieu.MatKhau,
                    HoTen = duLieu.HoTen,
                    SoDienThoai = duLieu.SoDienThoai
                };

                int maNguoiDung = await nguoiDungService.ThemNguoiDung(nguoiDung);
                return Ok(new { MaNguoiDung = maNguoiDung, ThongBao = "Thêm người dùng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpPut("{maNguoiDung}")]
        public async Task<IActionResult> CapNhatNguoiDung(int maNguoiDung, CapNhatNguoiDungDTO duLieu)
        {
            try
            {
                NguoiDung nguoiDung = new NguoiDung
                {
                    MaNguoiDung = maNguoiDung,
                    Email = duLieu.Email,
                    HoTen = duLieu.HoTen,
                    SoDienThoai = duLieu.SoDienThoai,
                    TrangThai = duLieu.TrangThai
                };
                int soDong = await nguoiDungService.CapNhatNguoiDung(nguoiDung);
                if (soDong == 0)
                {
                    return NotFound(new { ThongBao = "Không tìm thấy người dùng" });
                }
                return Ok(new { MaNguoiDung = maNguoiDung, ThongBao = "Cập nhật người dùng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpDelete("{maNguoiDung}")]
        public async Task<IActionResult> XoaMemNguoiDung(int maNguoiDung)
        {
            int soDong = await nguoiDungService.XoaMemNguoiDung(maNguoiDung);
            if (soDong == 0)
            {
                return NotFound(new { ThongBao = "Không tìm thấy người dùng" });
            }
            return Ok(new { MaNguoiDung = maNguoiDung, ThongBao = "Xóa người dùng thành công" });
        }
    }
}
