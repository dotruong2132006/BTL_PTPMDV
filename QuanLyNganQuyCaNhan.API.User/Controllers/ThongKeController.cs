using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyNganQuyCaNhan.BLL.Interfaces;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThongKeController : ControllerBase
    {
        private readonly IThongKeService thongKeService;
        public ThongKeController(IThongKeService thongKeService) { this.thongKeService = thongKeService; }

        private int LayMaNguoiDungTuToken()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int maNguoiDung))
                throw new ArgumentException("Không xác định được người dùng");
            return maNguoiDung;
        }

        [HttpGet("tong-quan")]
        public async Task<IActionResult> LayTongQuan([FromQuery] int thang, [FromQuery] int nam)
        {
            try { return Ok(await thongKeService.LayTongQuan(LayMaNguoiDungTuToken(), thang, nam, HttpContext.RequestAborted)); }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet("theo-thang")]
        public async Task<IActionResult> LayTheoThang([FromQuery] int nam, [FromQuery] int? maVi = null)
        {
            try { return Ok(await thongKeService.LayTheoThang(LayMaNguoiDungTuToken(), nam, maVi, HttpContext.RequestAborted)); }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet("theo-danh-muc")]
        public async Task<IActionResult> LayTheoDanhMuc([FromQuery] int thang, [FromQuery] int nam, [FromQuery] int? maVi = null)
        {
            try { return Ok(await thongKeService.LayTheoDanhMuc(LayMaNguoiDungTuToken(), thang, nam, maVi, HttpContext.RequestAborted)); }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }
    }
}
