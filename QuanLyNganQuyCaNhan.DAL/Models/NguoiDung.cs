using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class NguoiDung
    {
        public int MaNguoiDung { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MatKhau { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public bool DaBat2FA { get; set; }

        public string MaBiMat2FA { get; set; } = string.Empty;

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public DateTime? NgayXoa { get; set; }
    }
}
