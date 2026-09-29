using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class MatKhauService
    {
        public string BamMatKhau(string matKhau)
        {
            return BCrypt.Net.BCrypt.HashPassword(matKhau);
        }
        public bool KiemTraMatKhau(string matKhauNhap,string matKhauDaBam)
        {
            return BCrypt.Net.BCrypt.Verify(matKhauNhap,matKhauDaBam);
        }
    }
}
