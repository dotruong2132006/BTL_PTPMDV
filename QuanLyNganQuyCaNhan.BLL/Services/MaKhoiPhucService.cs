using System.Security.Cryptography;
using System.Text;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class MaKhoiPhucService
    {
        private const string LoiKhoiPhuc = "Thông tin khôi phục không hợp lệ hoặc mã đã được sử dụng/vô hiệu hóa";
        private readonly MaKhoiPhucRepository maKhoiPhucRepository;
        private readonly NguoiDungRepository nguoiDungRepository;
        private readonly MatKhauService matKhauService;
        private readonly HaiFaService haiFaService;

        public MaKhoiPhucService(MaKhoiPhucRepository maKhoiPhucRepository, NguoiDungRepository nguoiDungRepository,
            MatKhauService matKhauService, HaiFaService haiFaService)
        {
            this.maKhoiPhucRepository = maKhoiPhucRepository;
            this.nguoiDungRepository = nguoiDungRepository;
            this.matKhauService = matKhauService;
            this.haiFaService = haiFaService;
        }

        public Task<int> LaySoMaConLai(int maNguoiDung)
        {
            return maKhoiPhucRepository.LaySoMaConLai(maNguoiDung);
        }

        public async Task<IReadOnlyCollection<string>> TaoBoMa(int maNguoiDung, string matKhauHienTai,
            string? maOTP, string? diaChiIP)
        {
            var nguoiDung = await nguoiDungRepository.LayTheoMaNguoiDung(maNguoiDung);
            if (nguoiDung == null || !nguoiDung.TrangThai
                || !matKhauService.KiemTraMatKhau(matKhauHienTai, nguoiDung.MatKhau)
                || !KiemTra2FA(nguoiDung, maOTP))
                throw new ArgumentException("Không thể tạo mã khôi phục; kiểm tra mật khẩu hiện tại và mã 2FA nếu đã bật");

            var cacMa = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                string ma = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                cacMa.Add(string.Join("-", Enumerable.Range(0, 8).Select(x => ma.Substring(x * 4, 4))));
            }
            var cacMaDaBam = cacMa.Select(BamMa).ToList();
            if (!await maKhoiPhucRepository.TaoBoMa(nguoiDung, cacMaDaBam, diaChiIP))
                throw new ArgumentException("Thông tin bảo mật đã thay đổi; vui lòng đăng nhập lại và thử lại");
            return cacMa;
        }

        public async Task DatLaiMatKhau(string tenDangNhap, string maKhoiPhuc, string matKhauMoi,
            string? maOTP, string? diaChiIP)
        {
            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 8 || Encoding.UTF8.GetByteCount(matKhauMoi) > 72)
                throw new ArgumentException("Mật khẩu mới phải có ít nhất 8 ký tự và không vượt quá 72 byte UTF-8");
            string maChuanHoa = ChuanHoaMa(maKhoiPhuc);
            if (maChuanHoa.Length != 32 || !maChuanHoa.All(Uri.IsHexDigit))
                throw new ArgumentException(LoiKhoiPhuc);

            string matKhauDaBam = matKhauService.BamMatKhau(matKhauMoi);
            var nguoiDung = await nguoiDungRepository.LayTheoTenDangNhap(tenDangNhap.Trim());
            if (nguoiDung == null || !nguoiDung.TrangThai || !KiemTra2FA(nguoiDung, maOTP))
                throw new ArgumentException(LoiKhoiPhuc);

            if (!await maKhoiPhucRepository.DatLaiMatKhau(nguoiDung, BamMa(maChuanHoa), matKhauDaBam, diaChiIP))
                throw new ArgumentException(LoiKhoiPhuc);
        }

        private bool KiemTra2FA(NguoiDung nguoiDung, string? maOTP)
        {
            if (!nguoiDung.DaBat2FA) return true;
            if (string.IsNullOrWhiteSpace(nguoiDung.MaBiMat2FA) || string.IsNullOrWhiteSpace(maOTP)) return false;
            return haiFaService.KiemTraMaOTP(nguoiDung.MaBiMat2FA, maOTP.Trim());
        }

        private static string ChuanHoaMa(string ma)
        {
            return ma.Trim().Replace("-", "").Replace(" ", "").ToUpperInvariant();
        }

        private static byte[] BamMa(string ma)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(ChuanHoaMa(ma)));
        }
    }
}
