using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace QuanLyNganQuyCaNhan.API.Admin.Services
{
    public class JwtService
    {
        private readonly IConfiguration cauHinh;

        public JwtService(IConfiguration cauHinh)
        {
            this.cauHinh = cauHinh;
        }

        public string TaoToken(int maNguoiDung,string tenDangNhap, string vaiTro)
        {
            string khoaBiMat =cauHinh["Jwt:Key"]!;
            string issuer =cauHinh["Jwt:Issuer"]!;
            string audience =cauHinh["Jwt:Audience"]!;
            int thoiGianHetHan =int.Parse(cauHinh["Jwt:ThoiGianHetHanPhut"]!);
            var danhSachClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,maNguoiDung.ToString()),
                new Claim(ClaimTypes.Name,tenDangNhap),
                new Claim(ClaimTypes.Role,vaiTro)
            };
            var khoa =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(khoaBiMat));
            var thongTinKy =new SigningCredentials(khoa,SecurityAlgorithms.HmacSha256);
            var token =new JwtSecurityToken(issuer: issuer,audience: audience,claims: danhSachClaims,expires: DateTime.UtcNow.AddMinutes(thoiGianHetHan),signingCredentials: thongTinKy);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string TaoRefreshToken()
        {
            byte[] duLieuNgauNhien =RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(duLieuNgauNhien);
        }
    }
}