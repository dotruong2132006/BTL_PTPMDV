using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNganQuyCaNhan.API.User.DTOs;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using System.Security.Claims;

namespace QuanLyNganQuyCaNhan.API.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DanhMucController : ControllerBase
    {
        private readonly IDanhMucService danhMucService;

        public DanhMucController(IDanhMucService danhMucService)
        {
            this.danhMucService = danhMucService;
        }
        private int LayMaNguoiDungTuToken()
        {
            string maNguoiDung =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (!int.TryParse(maNguoiDung, out int maNguoiDungInt))
            {
                throw new Exception("Không xác định được người dùng");
            }
            return maNguoiDungInt;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSach()
        {
            int maNguoiDung = LayMaNguoiDungTuToken();

            var danhSach =await danhMucService.LayDanhSach(maNguoiDung);
            return Ok(danhSach);
        }

        [HttpPost]
        public async Task<IActionResult> ThemDanhMuc([FromBody] ThemDanhMucDTO duLieu)
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                DanhMuc danhMuc = new DanhMuc
                {
                    MaNguoiDung = maNguoiDung,
                    TenDanhMuc = duLieu.TenDanhMuc,
                    LoaiDanhMuc = duLieu.LoaiDanhMuc,
                    MoTa = duLieu.MoTa
                };
                int maDanhMuc =await danhMucService.ThemDanhMuc(danhMuc);
                return Ok(new{MaDanhMuc = maDanhMuc,ThongBao = "Thêm danh mục thành công"});
            }
            catch (Exception ex)
            {
                return BadRequest(new{ThongBao = ex.Message});
            }
        }

        [HttpPut("{maDanhMuc:int}")]
        public async Task<IActionResult> CapNhatDanhMuc(int maDanhMuc, [FromBody] CapNhatDanhMucDTO duLieu)
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                DanhMuc danhMuc = new DanhMuc
                {
                    MaDanhMuc = maDanhMuc,
                    MaNguoiDung = maNguoiDung,
                    TenDanhMuc = duLieu.TenDanhMuc,
                    LoaiDanhMuc = duLieu.LoaiDanhMuc,
                    MoTa = duLieu.MoTa
                };

                int soDong = await danhMucService.CapNhatDanhMuc(danhMuc);
                if (soDong == 0)
                {
                    return NotFound(new { ThongBao = "Không tìm thấy danh mục" });
                }

                return Ok(new { MaDanhMuc = maDanhMuc, ThongBao = "Cập nhật danh mục thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }

        [HttpDelete("{maDanhMuc:int}")]
        public async Task<IActionResult> XoaMemDanhMuc(int maDanhMuc)
        {
            try
            {
                int maNguoiDung = LayMaNguoiDungTuToken();
                int soDong = await danhMucService.XoaMemDanhMuc(maDanhMuc, maNguoiDung);
                if (soDong == 0)
                {
                    return NotFound(new { ThongBao = "Không tìm thấy danh mục" });
                }

                return Ok(new { MaDanhMuc = maDanhMuc, ThongBao = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ThongBao = ex.Message });
            }
        }
    }
}
