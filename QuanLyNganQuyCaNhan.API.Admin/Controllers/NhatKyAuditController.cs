using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class NhatKyAuditController : ControllerBase
    {
        private readonly INhatKyAuditService nhatKyAuditService;
        public NhatKyAuditController(INhatKyAuditService nhatKyAuditService) { this.nhatKyAuditService = nhatKyAuditService; }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach([FromQuery] BoLocNhatKyAudit boLoc)
        {
            try
            {
                var ketQua = await nhatKyAuditService.LayDanhSach(boLoc);
                Response.Headers.CacheControl = "no-store";
                return Ok(new { boLoc.Trang, boLoc.KichThuocTrang, ketQua.TongSoBanGhi,
                    TongSoTrang = (long)Math.Ceiling((decimal)ketQua.TongSoBanGhi / boLoc.KichThuocTrang),
                    DanhSach = ketQua.DanhSach.Select(x => new { x.MaAudit, x.MaNguoiDung, x.HanhDong,
                        x.TenBang, x.MaBanGhi, x.DiaChiIP, x.ThoiGian }) });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet("{maAudit:long}")]
        public async Task<IActionResult> LayTheoMa(long maAudit)
        {
            var nhatKy = await nhatKyAuditService.LayTheoMa(maAudit);
            if (nhatKy == null) return NotFound(new { ThongBao = "Không tìm thấy nhật ký" });
            Response.Headers.CacheControl = "no-store";
            return Ok(nhatKy);
        }
    }
}
