using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using QuanLyNganQuyCaNhan.DTOs;
using QuanLyNganQuyCaNhan.BLL.Services;

namespace QuanLyNganQuyCaNhan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class QuenMatKhauController : ControllerBase
    {
        private const string NhacLuuMa = "QUAN TRỌNG: Bạn phải lưu lại toàn bộ 10 mã khôi phục ở nơi an toàn ngay bây giờ. "
            + "Bộ mã chỉ hiển thị một lần và không thể xem lại. Mỗi mã chỉ dùng được một lần. "
            + "Khi tạo bộ mới, toàn bộ mã cũ sẽ bị vô hiệu hóa. "
            + "Nếu quên mật khẩu và mất hoặc dùng hết mã, bạn không thể tự khôi phục tài khoản và phải liên hệ admin để được hỗ trợ xác minh.";
        private readonly MaKhoiPhucService maKhoiPhucService;

        public QuenMatKhauController(MaKhoiPhucService maKhoiPhucService)
        {
            this.maKhoiPhucService = maKhoiPhucService;
        }

        [Authorize]
        [EnableRateLimiting("KhoiPhucTaiKhoan")]
        [HttpPost("tao-ma-khoi-phuc")]
        public async Task<IActionResult> TaoMaKhoiPhuc([FromBody] TaoMaKhoiPhucDTO duLieu)
        {
            if (!duLieu.DaHieuVaSeLuuMa)
                return BadRequest(new { ThongBao = NhacLuuMa, CanXacNhanLuuMa = true });
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int maNguoiDung))
                return Unauthorized(new { ThongBao = "Không xác định được người dùng" });

            try
            {
                var cacMa = await maKhoiPhucService.TaoBoMa(maNguoiDung, duLieu.MatKhauHienTai,
                    duLieu.MaOTP, HttpContext.Connection.RemoteIpAddress?.ToString());
                return Ok(new
                {
                    ThongBao = "Tạo bộ mã khôi phục thành công",
                    MaKhoiPhuc = cacMa,
                    ChiHienThiMotLan = true,
                    CanhBaoQuanTrong = new
                    {
                        MucDo = "QUAN_TRONG",
                        TieuDe = "BẮT BUỘC LƯU LẠI BỘ MÃ KHÔI PHỤC",
                        NoiDung = NhacLuuMa,
                        YeuCauXacNhanDaLuu = true
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("so-ma-con-lai")]
        public async Task<IActionResult> LaySoMaConLai()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int maNguoiDung))
                return Unauthorized(new { ThongBao = "Không xác định được người dùng" });
            int soMa = await maKhoiPhucService.LaySoMaConLai(maNguoiDung);
            return Ok(new
            {
                SoMaConLai = soMa,
                NenTaoLaiBoMa = soMa <= 2,
                ThongBao = soMa <= 2 ? "Bạn còn ít hoặc chưa có mã khôi phục. Hãy tạo bộ mã mới và lưu lại ở nơi an toàn." : null
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("KhoiPhucTaiKhoan")]
        [HttpPost("dat-lai")]
        public async Task<IActionResult> DatLaiMatKhau([FromBody] DatLaiMatKhauDTO duLieu)
        {
            try
            {
                await maKhoiPhucService.DatLaiMatKhau(duLieu.TenDangNhap, duLieu.MaKhoiPhuc,
                    duLieu.MatKhauMoi, duLieu.MaOTP, HttpContext.Connection.RemoteIpAddress?.ToString());
                return Ok(new
                {
                    ThongBao = "Đặt lại mật khẩu thành công. Mã khôi phục vừa dùng đã bị vô hiệu hóa; hãy đăng nhập bằng mật khẩu mới.",
                    NhacNho = "Sau khi đăng nhập, kiểm tra số mã còn lại và tạo bộ mã mới nếu cần. Bạn phải lưu lại bộ mã mới ở nơi an toàn."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }
    }
}
