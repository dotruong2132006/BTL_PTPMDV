using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class DangKyController : ControllerBase
    {
        private readonly INguoiDungService nguoiDungService;

        public DangKyController(INguoiDungService nguoiDungService)
        {
            this.nguoiDungService = nguoiDungService;
        }

        [HttpPost]
        public async Task<IActionResult> DangKy([FromBody] DangKyNguoiDungDTO duLieu)
        {
            try
            {
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = duLieu.TenDangNhap,
                    Email = duLieu.Email,
                    MatKhau = duLieu.MatKhau,
                    HoTen = duLieu.HoTen,
                    SoDienThoai = duLieu.SoDienThoai ?? string.Empty
                };

                int maNguoiDung = await nguoiDungService.DangKyNguoiDung(nguoiDung);
                return Ok(new
                {
                    MaNguoiDung = maNguoiDung,
                    ThongBao = "Đăng ký tài khoản thành công"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Tên đăng nhập hoặc email đã được sử dụng")
            {
                return Conflict(new { ThongBao = ex.Message });
            }
            catch (Exception)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Không thể tạo tài khoản lúc này");
            }
        }
    }
}
