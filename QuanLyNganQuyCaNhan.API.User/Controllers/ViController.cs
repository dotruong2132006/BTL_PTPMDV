using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ViController : ControllerBase
    {
        private readonly IViService viService;
        public ViController(IViService viService)
        {
            this.viService = viService;

        }
        private int LayMaNguoiDungTuToken()
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung,out int maNguoiDungInt))
            {
                throw new Exception("Không xác định được người dùng");
            }
            return maNguoiDungInt;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach()
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                var danhSach =await viService.LayDanhSach(maNguoiDung);
                return Ok(danhSach);
            }
            catch (Exception ex)
            {
                return BadRequest(new{ThongBao = ex.Message});
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThemVi(ThemViDTO duLieu)
        {
            try
            {
                int maNguoiDung =LayMaNguoiDungTuToken();
                Vi vi = new Vi{MaNguoiDung = maNguoiDung,TenVi = duLieu.TenVi};
                int maVi = await viService.ThemVi(vi);
                return Ok(new { MaVi = maVi, ThongBao = "Thêm ví thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpPut("{maVi}")]
        public async Task<IActionResult> CapNhatVi(int maVi, CapNhatViDTO duLieu)
        {
            try
            {
                int maNguoiDung =LayMaNguoiDungTuToken();
                Vi vi = new Vi{MaVi = maVi,MaNguoiDung = maNguoiDung,TenVi = duLieu.TenVi,TrangThai = duLieu.TrangThai};
                int soDong = await viService.CapNhatVi(vi);
                if (soDong == 0)
                {
                    return NotFound(new { ThongBao = "Không tìm thấy ví" });
                }

                return Ok(new { MaVi = maVi, ThongBao = "Cập nhật ví thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpDelete("{maVi}")]
        public async Task<IActionResult> XoaMemVi(int maVi)
        {
            int soDong = await viService.XoaMemVi(maVi);
            if (soDong == 0)
            {
                return NotFound(new { ThongBao = "Không tìm thấy ví" });
            }
            return Ok(new { MaVi = maVi, ThongBao = "Xóa ví thành công" });
        }

    }
}
