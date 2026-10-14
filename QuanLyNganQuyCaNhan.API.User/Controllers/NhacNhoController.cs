using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NhacNhoController : ControllerBase
    {
        private readonly INhacNhoService nhacNhoService;
        public NhacNhoController(INhacNhoService nhacNhoService)
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
        public async Task<IActionResult> LayDanhSach([FromQuery] bool? trangThai)
        {
            return Ok(await nhacNhoService.LayDanhSach(LayMaNguoiDungTuToken(), trangThai));
        }

        [HttpGet("{maNhacNho:int}")]
        public async Task<IActionResult> LayTheoMa(int maNhacNho)
        {
            var nhacNho = await nhacNhoService.LayTheoMa(maNhacNho, LayMaNguoiDungTuToken());
            if (nhacNho == null) return NotFound(new { ThongBao = "Không tìm thấy nhắc nhở" });
            return Ok(nhacNho);
        }

        [HttpPost]
        public async Task<IActionResult> ThemNhacNho([FromBody] ThemNhacNhoDTO duLieu)
        {
            try
            {
                int maNhacNho = await nhacNhoService.ThemNhacNho(TaoNhacNho(duLieu));
                return Ok(new { MaNhacNho = maNhacNho, PhienBan = 1, ThongBao = "Thêm nhắc nhở thành công" });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpPut("{maNhacNho:int}")]
        public async Task<IActionResult> CapNhatNhacNho(int maNhacNho, [FromBody] CapNhatNhacNhoDTO duLieu)
        {
            try
            {
                var nhacNho = TaoNhacNho(duLieu);
                nhacNho.MaNhacNho = maNhacNho;
                nhacNho.PhienBan = duLieu.PhienBan;
                var hienTai = await nhacNhoService.LayTheoMa(maNhacNho, nhacNho.MaNguoiDung);
                if (hienTai == null) return NotFound(new { ThongBao = "Không tìm thấy nhắc nhở" });
                if (hienTai.PhienBan != duLieu.PhienBan || await nhacNhoService.CapNhatNhacNho(nhacNho) == 0)
                    return Conflict(new { ThongBao = "Nhắc nhở đã thay đổi; vui lòng tải lại dữ liệu" });
                return Ok(new { MaNhacNho = maNhacNho, PhienBan = duLieu.PhienBan + 1, ThongBao = "Cập nhật nhắc nhở thành công" });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpDelete("{maNhacNho:int}")]
        public async Task<IActionResult> XoaMemNhacNho(int maNhacNho)
        {
            int soDong = await nhacNhoService.XoaMemNhacNho(maNhacNho, LayMaNguoiDungTuToken());
            if (soDong == 0) return NotFound(new { ThongBao = "Không tìm thấy nhắc nhở" });
            return Ok(new { MaNhacNho = maNhacNho, ThongBao = "Xóa nhắc nhở thành công" });
        }

        private NhacNho TaoNhacNho(ThemNhacNhoDTO duLieu)
        {
            return new NhacNho { MaNguoiDung = LayMaNguoiDungTuToken(), TieuDe = duLieu.TieuDe,
                NoiDung = duLieu.NoiDung, NgayNhac = duLieu.NgayNhac.UtcDateTime,
                LapLai = duLieu.LapLai, KieuLapLai = duLieu.KieuLapLai, TrangThai = duLieu.TrangThai };
        }
    }
}
