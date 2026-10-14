using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Services;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/GiaoDich")]
    [Authorize]
    public class GiaoDichFileController : ControllerBase
    {
        private readonly GiaoDichFileService giaoDichFileService;
        public GiaoDichFileController(GiaoDichFileService giaoDichFileService)
        {
            this.giaoDichFileService = giaoDichFileService;
        }

        private int LayMaNguoiDungTuToken()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int maNguoiDung))
                throw new Exception("Không xác định được người dùng");
            return maNguoiDung;
        }

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> NhapFile([FromForm] NhapFileGiaoDichDTO duLieu)
        {
            try
            {
                if (duLieu.File == null || duLieu.File.Length == 0 || duLieu.File.Length > GiaoDichFileService.GioiHanDungLuong)
                    return BadRequest(new { ThongBao = "Chọn file có dữ liệu và không vượt quá 5 MB" });
                using var stream = new MemoryStream();
                await duLieu.File.CopyToAsync(stream, HttpContext.RequestAborted);
                var ketQua = await giaoDichFileService.NhapFile(LayMaNguoiDungTuToken(), duLieu.MaYeuCau,
                    duLieu.File.FileName, stream.ToArray(), HttpContext.Connection.RemoteIpAddress?.ToString());
                return Ok(new { ketQua.MaLanNhap, ketQua.SoDong, ketQua.DaNhapTruoc,
                    ThongBao = ketQua.DaNhapTruoc ? "Lần nhập đã xử lý trước đó; không tạo giao dịch trùng" : "Nhập giao dịch thành công",
                    NhacNho = "Tải lại ví và ngân sách để xem số dư và cảnh báo chi tiêu mới" });
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet("export")]
        public async Task<IActionResult> XuatFile([FromQuery] string dinhDang = "csv", [FromQuery] DateTime? tuNgay = null,
            [FromQuery] DateTime? denNgay = null, [FromQuery] int? maDanhMuc = null, [FromQuery] int? maVi = null)
        {
            try
            {
                byte[] duLieu = await giaoDichFileService.XuatFile(LayMaNguoiDungTuToken(), dinhDang, tuNgay, denNgay, maDanhMuc, maVi);
                return TraFile(duLieu, dinhDang, "giao-dich");
            }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        [HttpGet("mau-file")]
        public async Task<IActionResult> LayFileMau([FromQuery] string dinhDang = "csv")
        {
            try { return TraFile(await giaoDichFileService.TaoFileMau(dinhDang), dinhDang, "mau-giao-dich"); }
            catch (ArgumentException ex) { return BadRequest(new { ThongBao = ex.Message }); }
        }

        private IActionResult TraFile(byte[] duLieu, string dinhDang, string tenFile)
        {
            dinhDang = dinhDang.Trim().ToLowerInvariant();
            return File(duLieu, dinhDang == "csv" ? "text/csv; charset=utf-8"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenFile + "." + dinhDang);
        }
    }
}
