using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class RefreshToken
    {
        public long MaRefreshToken { get; set; }

        public int MaNguoiDung { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime NgayHetHan { get; set; }

        public bool DaThuHoi { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
