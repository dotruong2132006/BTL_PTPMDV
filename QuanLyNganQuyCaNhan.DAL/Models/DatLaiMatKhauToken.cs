using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class DatLaiMatKhauToken
    {
        public long MaDatLaiMatKhau { get; set; }

        public int MaNguoiDung { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime NgayHetHan { get; set; }

        public bool DaSuDung { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
