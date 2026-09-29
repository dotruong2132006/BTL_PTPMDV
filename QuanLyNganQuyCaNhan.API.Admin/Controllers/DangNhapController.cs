using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.Admin.DTOs;
using QuanLyNganQuyCaNhan.API.Admin.Services;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.BLL.Services;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DangNhapController : ControllerBase
    {
        private readonly INguoiDungService nguoiDungService;
        private readonly MatKhauService matKhauService;
        private readonly JwtService jwtService;
        private readonly IRefreshTokenService refreshTokenService;
        private readonly HaiFaService haiFaService;

        public DangNhapController(INguoiDungService nguoiDungService,MatKhauService matKhauService, JwtService jwtService, IRefreshTokenService refreshTokenService, HaiFaService haiFaService)
        {
            this.nguoiDungService = nguoiDungService;
            this.matKhauService = matKhauService;
            this.jwtService = jwtService;
            this.refreshTokenService = refreshTokenService;
            this.haiFaService = haiFaService;
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapDTO duLieu)
        {
            var nguoiDung =await nguoiDungService.LayTheoTenDangNhap(duLieu.TenDangNhap);
            if (nguoiDung == null)
            {
                return Unauthorized(new{ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng"});
            }
            if (!nguoiDung.TrangThai)
            {
                return Unauthorized(new{ThongBao = "Tài khoản đã bị khóa"});
            }
            bool matKhauDung =matKhauService.KiemTraMatKhau(duLieu.MatKhau,nguoiDung.MatKhau);

            if (!matKhauDung)
            {
                return Unauthorized(new{ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng"});
            }
            if (nguoiDung.DaBat2FA)
            {
                return Ok(new{ThongBao ="Mật khẩu đúng, vui lòng nhập mã OTP",YeuCau2FA = true,MaNguoiDung =nguoiDung.MaNguoiDung});
            }
            string? vaiTro =await nguoiDungService.LayVaiTro(nguoiDung.MaNguoiDung);
            if (string.IsNullOrEmpty(vaiTro))
            {
                return Unauthorized(new{ThongBao = "Tài khoản chưa được phân quyền"});
            }
            string token =jwtService.TaoToken(nguoiDung.MaNguoiDung,nguoiDung.TenDangNhap,vaiTro);
            string refreshToken =jwtService.TaoRefreshToken();
            var duLieuRefreshToken = new RefreshToken
            {
                MaNguoiDung = nguoiDung.MaNguoiDung,
                Token = refreshToken,
                NgayHetHan = DateTime.UtcNow.AddDays(7),
                DaThuHoi = false,
                NgayTao = DateTime.UtcNow
            };
            await refreshTokenService.ThemRefreshToken(duLieuRefreshToken);
            return Ok(new{ThongBao = "Đăng nhập thành công",MaNguoiDung = nguoiDung.MaNguoiDung,TenDangNhap = nguoiDung.TenDangNhap,HoTen = nguoiDung.HoTen,VaiTro = vaiTro,AccessToken = token, RefreshToken = refreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO duLieu)
        {
            var refreshToken =await refreshTokenService.LayTheoToken(duLieu.RefreshToken);
            if (refreshToken == null)
            {
                return Unauthorized(new{ThongBao = "Refresh Token không hợp lệ"});
            }
            if (refreshToken.DaThuHoi)
            {
                return Unauthorized(new{ThongBao = "Refresh Token đã bị thu hồi"});
            }
            if (refreshToken.NgayHetHan <= DateTime.UtcNow)
            {
                return Unauthorized(new{ThongBao = "Refresh Token đã hết hạn"});
            }
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(refreshToken.MaNguoiDung);
            if (nguoiDung == null)
            {
                return Unauthorized(new{ThongBao = "Người dùng không tồn tại"});
            }
            if (!nguoiDung.TrangThai)
            {
                return Unauthorized(new{ThongBao = "Tài khoản đã bị khóa"});
            }
            string? vaiTro =await nguoiDungService.LayVaiTro(nguoiDung.MaNguoiDung);
            if (string.IsNullOrEmpty(vaiTro))
            {
                return Unauthorized(new{ThongBao = "Tài khoản chưa được phân quyền"});
            }
            string accessToken =jwtService.TaoToken(nguoiDung.MaNguoiDung,nguoiDung.TenDangNhap,vaiTro);
            return Ok(new{ThongBao = "Tạo Access Token mới thành công",AccessToken = accessToken});
        }

        [HttpPost("logout")]
        public async Task<IActionResult> DangXuat(RefreshTokenDTO duLieu)
        {
            var refreshToken =await refreshTokenService.LayTheoToken(duLieu.RefreshToken);
            if (refreshToken == null)
            {
                return Unauthorized(new{ThongBao = "Refresh Token không hợp lệ"});
            }
            if (refreshToken.DaThuHoi)
            {
                return Unauthorized(new{ThongBao = "Refresh Token đã được thu hồi"});
            }
            await refreshTokenService.ThuHoiToken(refreshToken.MaRefreshToken);
            return Ok(new{ThongBao = "Đăng xuất thành công"});
        }
        [HttpPost("xacnhan-2fa")]
        public async Task<IActionResult> XacNhanDangNhap2FA(XacNhanDangNhap2FADTO duLieu)
        {
            var nguoiDung =await nguoiDungService.LayTheoMaNguoiDung(duLieu.MaNguoiDung);
            if (nguoiDung == null)
            {
                return Unauthorized(new{ThongBao = "Người dùng không tồn tại"});
            }
            if (!nguoiDung.TrangThai)
            {
                return Unauthorized(new{ThongBao = "Tài khoản đã bị khóa"});
            }
            if (!nguoiDung.DaBat2FA)
            {
                return BadRequest(new{ThongBao = "Tài khoản chưa bật 2FA"});
            }

            if (string.IsNullOrEmpty(
                nguoiDung.MaBiMat2FA))
            {
                return BadRequest(new{ThongBao ="Tài khoản chưa có mã bí mật 2FA"});
            }
            bool maOTPDung =haiFaService.KiemTraMaOTP(nguoiDung.MaBiMat2FA,duLieu.MaOTP);
            if (!maOTPDung)
            {
                return Unauthorized(new{ThongBao ="Mã OTP không đúng hoặc đã hết hạn"});
            }
            string? vaiTro =await nguoiDungService.LayVaiTro(nguoiDung.MaNguoiDung);
            if (string.IsNullOrEmpty(vaiTro))
            {
                return Unauthorized(new{ThongBao ="Tài khoản chưa được phân quyền"});
            }
            string accessToken =jwtService.TaoToken(nguoiDung.MaNguoiDung,nguoiDung.TenDangNhap,vaiTro);
            string refreshToken =jwtService.TaoRefreshToken();
            var duLieuRefreshToken =
                new RefreshToken
                {
                    MaNguoiDung =nguoiDung.MaNguoiDung,
                    Token =refreshToken,
                    NgayHetHan =DateTime.UtcNow.AddDays(7),
                    DaThuHoi = false,
                    NgayTao =DateTime.UtcNow
                };
            await refreshTokenService.ThemRefreshToken(duLieuRefreshToken);
            return Ok(new{ThongBao ="Đăng nhập 2FA thành công",MaNguoiDung =nguoiDung.MaNguoiDung,TenDangNhap =nguoiDung.TenDangNhap,HoTen =nguoiDung.HoTen,VaiTro =vaiTro,AccessToken =accessToken,RefreshToken =refreshToken});
        }
    }
}