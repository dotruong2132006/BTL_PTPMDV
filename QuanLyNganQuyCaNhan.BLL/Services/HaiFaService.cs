using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtpNet;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class HaiFaService
    {
        public string TaoMaBiMat()
        {
            byte[] maBiMat =KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(maBiMat);
        }

        public string TaoMaOTP(string maBiMat)
        {
            byte[] duLieu =Base32Encoding.ToBytes(maBiMat);
            var totp =new Totp(duLieu);
            return totp.ComputeTotp();
        }

        public bool KiemTraMaOTP(string maBiMat,string maOTP)
        {
            byte[] duLieu =Base32Encoding.ToBytes(maBiMat);
            var totp =new Totp(duLieu);
            return totp.VerifyTotp(maOTP,out long buocThoiGian);
        }
    }
}
