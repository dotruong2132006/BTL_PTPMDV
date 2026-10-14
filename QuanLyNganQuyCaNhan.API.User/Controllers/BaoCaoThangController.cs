using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BaoCaoThangController : ControllerBase
    {
        private readonly IBaoCaoThangService baoCaoThangService;
        public BaoCaoThangController(IBaoCaoThangService baoCaoThangService)
        {
            this.baoCaoThangService = baoCaoThangService;
        }

        private int LayMaNguoiDungTuToken()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int maNguoiDung))
                throw new Exception("Không xác định được người dùng");
            return maNguoiDung;
        }

        [HttpPost]
        public async Task<IActionResult> TaoYeuCau([FromBody] TaoBaoCaoThangDTO duLieu)
        {
            try
            {
                var baoCao = await baoCaoThangService.TaoYeuCau(LayMaNguoiDungTuToken(), duLieu.Thang, duLieu.Nam,
                    duLieu.MaYeuCau, HttpContext.Connection.RemoteIpAddress?.ToString());
                return Ok(new { baoCao.MaBaoCao, baoCao.TrangThai,
                    ThongBao = "Yêu cầu đã được tiếp nhận; theo dõi trạng thái rồi tải PDF khi hoàn thành",
                    UrlTrangThai = $"/api/BaoCaoThang/{baoCao.MaBaoCao}", UrlTaiFile = $"/api/BaoCaoThang/{baoCao.MaBaoCao}/tai-file" });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach([FromQuery] int trang = 1, [FromQuery] int kichThuocTrang = 20)
        {
            try
            {
                var ketQua = await baoCaoThangService.LayDanhSach(LayMaNguoiDungTuToken(), trang, kichThuocTrang);
                return Ok(new { Trang = trang, KichThuocTrang = kichThuocTrang, ketQua.TongSoBanGhi, ketQua.DanhSach });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet("{maBaoCao:long}")]
        public async Task<IActionResult> LayTheoMa(long maBaoCao)
        {
            var baoCao = await baoCaoThangService.LayTheoMa(maBaoCao, LayMaNguoiDungTuToken());
            if (baoCao == null) return NotFound(new { ThongBao = "Không tìm thấy báo cáo" });
            return Ok(baoCao);
        }

        [HttpGet("{maBaoCao:long}/tai-file")]
        public async Task<IActionResult> TaiFile(long maBaoCao)
        {
            int maNguoiDung = LayMaNguoiDungTuToken();
            var baoCao = await baoCaoThangService.LayTheoMa(maBaoCao, maNguoiDung);
            if (baoCao == null) return NotFound(new { ThongBao = "Không tìm thấy báo cáo" });
            if (baoCao.TrangThai != "HOAN_THANH") return Conflict(new { baoCao.TrangThai,
                ThongBao = baoCao.TrangThai == "LOI" ? baoCao.LoiCuoi : "Báo cáo chưa hoàn thành; vui lòng chờ" });
            byte[]? file = await baoCaoThangService.LayFile(maBaoCao, maNguoiDung);
            if (file == null) return NotFound(new { ThongBao = "Không tìm thấy file hoặc tài khoản không còn hoạt động" });
            Response.Headers["Cache-Control"] = "no-store";
            return File(file, "application/pdf", $"bao-cao-thang-{baoCao.Thang:00}-{baoCao.Nam}.pdf");
        }
    }
}
