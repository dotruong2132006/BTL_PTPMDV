using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyNganQuyCaNhan.BLL.Interfaces;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThongBaoNhacNhoController : ControllerBase
    {
        private readonly INhacNhoService nhacNhoService;
        public ThongBaoNhacNhoController(INhacNhoService nhacNhoService)
        {
            this.nhacNhoService = nhacNhoService;
        }

        private int LayMaNguoiDungTuToken()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int maNguoiDung))
                throw new Exception("Không xác định được người dùng");
            return maNguoiDung;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach([FromQuery] bool chiChuaDoc = false,
            [FromQuery] int trang = 1, [FromQuery] int kichThuocTrang = 20)
        {
            try
            {
                var ketQua = await nhacNhoService.LayThongBao(LayMaNguoiDungTuToken(), chiChuaDoc, trang, kichThuocTrang);
                return Ok(new { Trang = trang, KichThuocTrang = kichThuocTrang, ketQua.TongSoBanGhi, ketQua.DanhSach });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpPut("{maThongBao:long}/da-doc")]
        public async Task<IActionResult> DanhDauDaDoc(long maThongBao)
        {
            int soDong = await nhacNhoService.DanhDauDaDoc(maThongBao, LayMaNguoiDungTuToken());
            if (soDong == 0) return NotFound(new { ThongBao = "Không tìm thấy thông báo" });
            return Ok(new { MaThongBao = maThongBao, ThongBao = "Đã đánh dấu thông báo đã đọc" });
        }
    }
}
