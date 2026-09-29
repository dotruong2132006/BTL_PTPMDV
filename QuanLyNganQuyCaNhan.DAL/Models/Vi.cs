using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class Vi
    {
        public int MaVi { get; set; }

        public int MaNguoiDung { get; set; }

        public string TenVi { get; set; } = string.Empty;

        public decimal SoDu { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public DateTime? NgayXoa { get; set; }
    }
}
