using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.Admin.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DanhMucHeThongController : ControllerBase
    {
        private readonly IDanhMucHeThongService danhMucService;
        public DanhMucHeThongController(IDanhMucHeThongService danhMucService) { this.danhMucService = danhMucService; }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach([FromQuery] string? loaiDanhMuc = null)
        {
            try { return Ok(await danhMucService.LayDanhSach(loaiDanhMuc)); }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }
        [HttpGet("{maDanhMuc:int}")]
        public async Task<IActionResult> LayTheoMa(int maDanhMuc)
        {
            var danhMuc = await danhMucService.LayTheoMa(maDanhMuc);
            if (danhMuc == null) return NotFound(new { ThongBao = "Không tìm thấy danh mục hệ thống" });
            return Ok(danhMuc);
        }
        [HttpPost]
        public async Task<IActionResult> ThemDanhMuc([FromBody] ThemDanhMucHeThongDTO duLieu)
        {
            try
            {
                int maDanhMuc = await danhMucService.ThemDanhMuc(TaoDanhMuc(duLieu));
                return Ok(new { MaDanhMuc = maDanhMuc, ThongBao = "Thêm danh mục hệ thống thành công" });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }
        [HttpPut("{maDanhMuc:int}")]
        public async Task<IActionResult> CapNhatDanhMuc(int maDanhMuc, [FromBody] CapNhatDanhMucHeThongDTO duLieu)
        {
            try
            {
                var danhMuc = TaoDanhMuc(duLieu);
                danhMuc.MaDanhMuc = maDanhMuc;
                int soDong = await danhMucService.CapNhatDanhMuc(danhMuc);
                if (soDong == 0) return NotFound(new { ThongBao = "Không tìm thấy danh mục hệ thống" });
                return Ok(new { MaDanhMuc = maDanhMuc, ThongBao = "Cập nhật danh mục hệ thống thành công" });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }
        [HttpDelete("{maDanhMuc:int}")]
        public async Task<IActionResult> XoaMemDanhMuc(int maDanhMuc)
        {
            int soDong = await danhMucService.XoaMemDanhMuc(maDanhMuc);
            if (soDong == 0) return NotFound(new { ThongBao = "Không tìm thấy danh mục hệ thống" });
            return Ok(new { MaDanhMuc = maDanhMuc, ThongBao = "Xóa danh mục hệ thống thành công" });
        }
        private static DanhMuc TaoDanhMuc(ThemDanhMucHeThongDTO duLieu) => new()
        {
            TenDanhMuc = duLieu.TenDanhMuc, LoaiDanhMuc = duLieu.LoaiDanhMuc, MoTa = duLieu.MoTa
        };
    }
}
