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
    public class GiaoDichController : ControllerBase
    {
        private readonly IGiaoDichService giaoDichService;
        private readonly INganSachService nganSachService;
        private readonly ILogger<GiaoDichController> logger;

        public GiaoDichController(IGiaoDichService giaoDichService, INganSachService nganSachService,ILogger<GiaoDichController> logger)
        {
            this.giaoDichService = giaoDichService;
            this.nganSachService = nganSachService;
            this.logger = logger;
        }

        private int LayMaNguoiDungTuToken()
        {
            string maNguoiDung = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung, out int maNguoiDungInt))
            {
                throw new Exception("Không xác định được người dùng");
            }
            return maNguoiDungInt;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach(
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay,
            [FromQuery] int? maDanhMuc,
            [FromQuery] int trang = 1,
            [FromQuery] int kichThuocTrang = 20,
            [FromQuery] string sapXepTheo = "NgayGiaoDich",
            [FromQuery] string chieuSapXep = "DESC")
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                var ketQua = await giaoDichService.LayDanhSach(
                    maNguoiDung, tuNgay, denNgay, maDanhMuc, trang, kichThuocTrang, sapXepTheo, chieuSapXep);

                return Ok(new
                {
                    Trang = trang,
                    KichThuocTrang = kichThuocTrang,
                    TongSoBanGhi = ketQua.TongSoBanGhi,
                    DanhSach = ketQua.DanhSach
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpGet("{maGiaoDich:long}")]
        public async Task<IActionResult> LayTheoMa(long maGiaoDich)
        {
            int maNguoiDung = LayMaNguoiDungTuToken();
            var giaoDich = await giaoDichService.LayTheoMa(maGiaoDich, maNguoiDung);
            if (giaoDich == null)
            {
                return NotFound(new { ThongBao = "Không tìm thấy giao dịch" });
            }
            return Ok(giaoDich);
        }

        [HttpPost]
        public async Task<IActionResult> ThemGiaoDich([FromBody] ThemGiaoDichDTO duLieu)
        {
            try
            {
                var giaoDich = new GiaoDich
                {
                    MaNguoiDung = LayMaNguoiDungTuToken(),
                    MaVi = duLieu.MaVi,
                    MaDanhMuc = duLieu.MaDanhMuc,
                    LoaiGiaoDich = duLieu.LoaiGiaoDich,
                    SoTien = duLieu.SoTien,
                    NoiDung = duLieu.NoiDung,
                    NgayGiaoDich = duLieu.NgayGiaoDich
                };

                long maGiaoDich = await giaoDichService.ThemGiaoDich(giaoDich);
                var canhBao = await LayCanhBaoSauKhiLuu(giaoDich);
                return Ok(new
                {
                    MaGiaoDich = maGiaoDich,
                    PhienBan = 1,
                    ThongBao = "Thêm giao dịch thành công",
                    CanhBaoNganSach = canhBao,
                    KiemTraCanhBaoThanhCong = canhBao != null
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpPut("{maGiaoDich:long}")]
        public async Task<IActionResult> CapNhatGiaoDich(long maGiaoDich, [FromBody] CapNhatGiaoDichDTO duLieu)
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                var giaoDichHienTai = await giaoDichService.LayTheoMa(maGiaoDich, maNguoiDung);
                if (giaoDichHienTai == null)
                {
                    return NotFound(new { ThongBao = "Không tìm thấy giao dịch" });
                }
                if (giaoDichHienTai.PhienBan != duLieu.PhienBan)
                {
                    return Conflict(new { ThongBao = "Giao dịch đã được cập nhật; vui lòng tải lại dữ liệu" });
                }

                var giaoDich = new GiaoDich
                {
                    MaGiaoDich = maGiaoDich,
                    MaNguoiDung = maNguoiDung,
                    MaVi = duLieu.MaVi,
                    MaDanhMuc = duLieu.MaDanhMuc,
                    LoaiGiaoDich = duLieu.LoaiGiaoDich,
                    SoTien = duLieu.SoTien,
                    NoiDung = duLieu.NoiDung,
                    NgayGiaoDich = duLieu.NgayGiaoDich,
                    PhienBan = duLieu.PhienBan
                };

                int soDong = await giaoDichService.CapNhatGiaoDich(giaoDich);
                if (soDong == 0)
                {
                    return Conflict(new { ThongBao = "Giao dịch đã được cập nhật; vui lòng tải lại dữ liệu" });
                }

                var canhBao = await LayCanhBaoSauKhiLuu(giaoDich);
                return Ok(new
                {
                    MaGiaoDich = maGiaoDich,
                    PhienBan = duLieu.PhienBan + 1,
                    ThongBao = "Cập nhật giao dịch thành công",
                    CanhBaoNganSach = canhBao,
                    KiemTraCanhBaoThanhCong = canhBao != null
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpDelete("{maGiaoDich:long}")]
        public async Task<IActionResult> XoaMemGiaoDich(long maGiaoDich)
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                int soDong = await giaoDichService.XoaMemGiaoDich(maGiaoDich, maNguoiDung);
                if (soDong == 0)
                {
                    return NotFound(new { ThongBao = "Không tìm thấy giao dịch" });
                }
                return Ok(new { MaGiaoDich = maGiaoDich, ThongBao = "Xóa giao dịch thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        private async Task<IEnumerable<TinhTrangNganSach>?> LayCanhBaoSauKhiLuu(GiaoDich giaoDich)
        {
            if (giaoDich.LoaiGiaoDich != "CHI")
                return Array.Empty<TinhTrangNganSach>();

            try
            {
                return await nganSachService.LayCanhBaoVuotNganSach(
                    giaoDich.MaNguoiDung, giaoDich.MaDanhMuc, giaoDich.NgayGiaoDich);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Không đọc được cảnh báo ngân sách sau khi lưu giao dịch");
                return null;
            }
        }
    }
}
