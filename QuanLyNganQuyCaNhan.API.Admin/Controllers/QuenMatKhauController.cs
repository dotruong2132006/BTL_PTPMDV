using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.Admin.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuenMatKhauController : ControllerBase
    {
        private readonly INguoiDungService nguoiDungService;
        private readonly IDatLaiMatKhauTokenService datLaiMatKhauTokenService;
        private readonly IRefreshTokenService refreshTokenService;
        public QuenMatKhauController(INguoiDungService nguoiDungService,IDatLaiMatKhauTokenService datLaiMatKhauTokenService, IRefreshTokenService refreshTokenService)
        {
            this.nguoiDungService =nguoiDungService;
            this.datLaiMatKhauTokenService =datLaiMatKhauTokenService;
            this.refreshTokenService =refreshTokenService;
        }
        [HttpPost("yeu-cau")]
        public async Task<IActionResult> YeuCauQuenMatKhau(YeuCauQuenMatKhauDTO duLieu)
        {
            if (string.IsNullOrEmpty(duLieu.Email))
            {
                return BadRequest(new{ThongBao ="Vui lòng nhập Email"});
            }
            var nguoiDung =await nguoiDungService.LayDanhSach();
            NguoiDung? nguoiDungTimThay = null;
            foreach (var nguoi in nguoiDung)
            {
                if (nguoi.Email == duLieu.Email)
                {
                    nguoiDungTimThay = nguoi;
                    break;
                }
            }

            if (nguoiDungTimThay == null)
            {
                return NotFound(new{ThongBao ="Email không tồn tại"});
            }
            byte[] duLieuNgauNhien =RandomNumberGenerator.GetBytes(64);
            string token =Convert.ToBase64String(duLieuNgauNhien);
            var duLieuToken =new DatLaiMatKhauToken{MaNguoiDung =nguoiDungTimThay.MaNguoiDung,Token =token,NgayHetHan =DateTime.UtcNow.AddMinutes(15),DaSuDung = false,NgayTao =DateTime.UtcNow};
            await datLaiMatKhauTokenService.ThemToken(duLieuToken);
            return Ok(new{ThongBao ="Tạo token đặt lại mật khẩu thành công",Token =token,NgayHetHan =duLieuToken.NgayHetHan});
        }
        [HttpPost("dat-lai")]
        public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauDTO duLieu)
        {
            if (string.IsNullOrEmpty(duLieu.Token))
            {
                return BadRequest(new{ThongBao ="Vui lòng nhập Token"});
            }
            if (string.IsNullOrEmpty(duLieu.MatKhauMoi))
            {
                return BadRequest(new{ThongBao ="Vui lòng nhập mật khẩu mới"});
            }
            var token =await datLaiMatKhauTokenService.LayTheoToken(duLieu.Token);
            if (token == null)
            {
                return BadRequest(new{ThongBao ="Token không tồn tại"});
            }
            if (token.DaSuDung)
            {
                return BadRequest(new{ThongBao ="Token đã được sử dụng"});
            }
            if (token.NgayHetHan < DateTime.UtcNow)
            {
                return BadRequest(new{ThongBao ="Token đã hết hạn"});
            }
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(token.MaNguoiDung);
            if (nguoiDung == null)
            {
                return NotFound(new{ThongBao ="Người dùng không tồn tại"});
            }
            int ketQua =await nguoiDungService.CapNhatMatKhau(token.MaNguoiDung,duLieu.MatKhauMoi);
            if (ketQua == 0)
            {
                return BadRequest(new{ThongBao ="Không thể cập nhật mật khẩu"});
            }
            await datLaiMatKhauTokenService.DanhDauDaSuDung(token.MaDatLaiMatKhau);
            await refreshTokenService.ThuHoiTatCaToken(token.MaNguoiDung);
            return Ok(new{ThongBao ="Đặt lại mật khẩu thành công"});
        }
    }
}