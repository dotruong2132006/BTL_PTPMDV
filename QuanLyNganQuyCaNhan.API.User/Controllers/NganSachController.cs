using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using System.Security.Claims;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NganSachController : ControllerBase
    {
        private readonly INganSachService nganSachService;

        public NganSachController(INganSachService nganSachService)
        {
            this.nganSachService = nganSachService;
        }

        private int LayMaNguoiDungTuToken()
        {
            string maNguoiDung = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung, out int maNguoiDungInt))
                throw new Exception("Không xác định được người dùng");
            return maNguoiDungInt;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach([FromQuery] int? thang, [FromQuery] int? nam, [FromQuery] int? maDanhMuc)
        {
            try
            {
                var danhSach = await nganSachService.LayDanhSach(LayMaNguoiDungTuToken(), thang, nam, maDanhMuc);
                return Ok(danhSach);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpGet("{maNganSach:int}")]
        public async Task<IActionResult> LayTheoMa(int maNganSach)
        {
            var nganSach = await nganSachService.LayTheoMa(maNganSach, LayMaNguoiDungTuToken());
            if (nganSach == null)
                return NotFound(new { ThongBao = "Không tìm thấy ngân sách" });
            return Ok(nganSach);
        }

        [HttpGet("canh-bao")]
        public async Task<IActionResult> LayCanhBao([FromQuery] int thang, [FromQuery] int nam)
        {
            try
            {
                var danhSach = await nganSachService.LayCanhBao(LayMaNguoiDungTuToken(), thang, nam);
                return Ok(danhSach);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThemNganSach([FromBody] ThemNganSachDTO duLieu)
        {
            try
            {
                var nganSach = TaoNganSach(duLieu);
                int maNganSach = await nganSachService.ThemNganSach(nganSach);
                return Ok(new { MaNganSach = maNganSach, ThongBao = "Thêm ngân sách thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpPut("{maNganSach:int}")]
        public async Task<IActionResult> CapNhatNganSach(int maNganSach, [FromBody] CapNhatNganSachDTO duLieu)
        {
            try
            {
                var nganSach = TaoNganSach(duLieu);
                nganSach.MaNganSach = maNganSach;
                int soDong = await nganSachService.CapNhatNganSach(nganSach);
                if (soDong == 0)
                    return NotFound(new { ThongBao = "Không tìm thấy ngân sách" });
                return Ok(new { MaNganSach = maNganSach, ThongBao = "Cập nhật ngân sách thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpDelete("{maNganSach:int}")]
        public async Task<IActionResult> XoaMemNganSach(int maNganSach)
        {
            int soDong = await nganSachService.XoaMemNganSach(maNganSach, LayMaNguoiDungTuToken());
            if (soDong == 0)
                return NotFound(new { ThongBao = "Không tìm thấy ngân sách" });
            return Ok(new { MaNganSach = maNganSach, ThongBao = "Xóa ngân sách thành công" });
        }

        private NganSach TaoNganSach(ThemNganSachDTO duLieu)
        {
            return new NganSach
            {
                MaNguoiDung = LayMaNguoiDungTuToken(),
                MaDanhMuc = duLieu.MaDanhMuc,
                SoTienGioiHan = duLieu.SoTienGioiHan,
                Thang = duLieu.Thang,
                Nam = duLieu.Nam,
                PhanTramCanhBao = duLieu.PhanTramCanhBao
            };
        }
    }
}
