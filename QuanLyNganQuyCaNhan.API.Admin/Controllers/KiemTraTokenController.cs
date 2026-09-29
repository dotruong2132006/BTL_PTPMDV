using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KiemTraTokenController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult KiemTraToken()
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            string tenDangNhap =User.FindFirst(ClaimTypes.Name)?.Value ?? "";
            return Ok(new{ThongBao = "Token hợp lệ",MaNguoiDung = maNguoiDung,TenDangNhap = tenDangNhap});
        }
    }
}