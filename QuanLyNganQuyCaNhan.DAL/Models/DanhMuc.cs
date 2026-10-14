using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class DanhMuc
    {
        public int MaDanhMuc { get; set; }

        public int? MaNguoiDung { get; set; }

        public bool LaDanhMucHeThong => !MaNguoiDung.HasValue;

        public string TenDanhMuc { get; set; } = string.Empty;

        public string LoaiDanhMuc { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayXoa { get; set; }
    }
}
