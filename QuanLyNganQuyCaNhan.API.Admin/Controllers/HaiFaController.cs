using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.BLL.Services;
using QRCoder;
using QuanLyNganQuyCaNhan.API.Admin.DTOs;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HaiFaController : ControllerBase
    {
        private readonly INguoiDungService nguoiDungService;
        private readonly HaiFaService haiFaService;
        public HaiFaController(INguoiDungService nguoiDungService,HaiFaService haiFaService)
        {
            this.nguoiDungService =nguoiDungService;
            this.haiFaService =haiFaService;
        }

        [HttpPost("bat")]
        public async Task<IActionResult> Bat2FA()
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung,out int maNguoiDungInt))
            {
                return Unauthorized(new{ThongBao ="Không xác định được người dùng"});
            }
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(maNguoiDungInt);
            if (nguoiDung == null)
            {
                return NotFound(new{ThongBao ="Người dùng không tồn tại"});
            }

            if (nguoiDung.DaBat2FA)
            {
                return BadRequest(new{ThongBao ="Tài khoản đã bật 2FA"});
            }
            string maBiMat =haiFaService.TaoMaBiMat();
            await nguoiDungService.Bat2FA(maNguoiDungInt,maBiMat);
            return Ok(new{ThongBao ="Bật 2FA thành công",MaBiMat2FA = maBiMat});
        }
        [HttpGet("qr")]
        public async Task<IActionResult> TaoQR()
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung,out int maNguoiDungInt))
            {
                return Unauthorized(new{ThongBao ="Không xác định được người dùng"});
            }
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(maNguoiDungInt);
            if (nguoiDung == null)
            {
                return NotFound(new{ThongBao ="Người dùng không tồn tại"});
            }
            if (!nguoiDung.DaBat2FA)
            {
                return BadRequest(new{ThongBao ="Tài khoản chưa bật 2FA"});
            }
            string noiDungQR =
                "otpauth://totp/" +
                "QuanLyNganQuyCaNhan:" +
                nguoiDung.TenDangNhap +
                "?secret=" +
                nguoiDung.MaBiMat2FA +
                "&issuer=" +
                "QuanLyNganQuyCaNhan";

            using var qrGenerator =new QRCodeGenerator();
            using var qrData =qrGenerator.CreateQrCode(noiDungQR,QRCodeGenerator.ECCLevel.Q);
            var qrCode =new PngByteQRCode(qrData);
            byte[] anhQR =qrCode.GetGraphic(20);
            return File(anhQR,"image/png");
        }
        [HttpPost("xacnhan")]
        public async Task<IActionResult> XacNhan2FA(XacNhan2FADTO duLieu)
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung,out int maNguoiDungInt))
            {
                return Unauthorized(new{ThongBao ="Không xác định được người dùng"});
            }
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(maNguoiDungInt);
            if (nguoiDung == null)
            {
                return NotFound(new{ThongBao ="Người dùng không tồn tại"});
            }
            if (!nguoiDung.DaBat2FA)
            {
                return BadRequest(new{ThongBao ="Tài khoản chưa bật 2FA"});
            }

            if (string.IsNullOrEmpty(nguoiDung.MaBiMat2FA))
            {
                return BadRequest(new{ThongBao ="Tài khoản chưa có mã bí mật 2FA"});
            }
            if (string.IsNullOrEmpty(duLieu.MaOTP))
            {
                return BadRequest(new{ThongBao ="Vui lòng nhập mã OTP"});
            }
            bool maOTPĐung =haiFaService.KiemTraMaOTP(nguoiDung.MaBiMat2FA,duLieu.MaOTP);
            if (!maOTPĐung)
            {
                return BadRequest(new{ThongBao ="Mã OTP không đúng hoặc đã hết hạn"});
            }
            return Ok(new{ThongBao ="Xác nhận 2FA thành công"});
        }
        [HttpPost("tat")]
        public async Task<IActionResult> Tat2FA()
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            if (!int.TryParse(maNguoiDung,out int maNguoiDungInt))
            {
                return Unauthorized(new{ThongBao ="Không xác định được người dùng"});
            }
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(maNguoiDungInt);
            if (nguoiDung == null)
            {
                return NotFound(new{ThongBao ="Người dùng không tồn tại"});
            }
            if (!nguoiDung.DaBat2FA)
            {
                return BadRequest(new{ThongBao ="Tài khoản chưa bật 2FA"});
            }
            await nguoiDungService.Tat2FA(maNguoiDungInt);
            return Ok(new{ThongBao ="Tắt 2FA thành công"});
        }
    }
}