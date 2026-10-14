using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class NganSachService : INganSachService
    {
        private readonly NganSachRepository nganSachRepository;

        public NganSachService(NganSachRepository nganSachRepository)
        {
            this.nganSachRepository = nganSachRepository;
        }

        public async Task<IEnumerable<TinhTrangNganSach>> LayDanhSach(
            int maNguoiDung, int? thang, int? nam, int? maDanhMuc)
        {
            KiemTraThangNam(thang, nam);
            if (maDanhMuc.HasValue && maDanhMuc.Value <= 0)
                throw new ArgumentException("Mã danh mục không hợp lệ");

            var danhSach = (await nganSachRepository.LayDanhSach(maNguoiDung, thang, nam, maDanhMuc)).ToList();
            foreach (var nganSach in danhSach)
                TinhTienDoVaCanhBao(nganSach);
            return danhSach;
        }

        public async Task<TinhTrangNganSach?> LayTheoMa(int maNganSach, int maNguoiDung)
        {
            var nganSach = (await nganSachRepository.LayDanhSach(
                maNguoiDung, null, null, null, maNganSach)).FirstOrDefault();
            if (nganSach != null)
                TinhTienDoVaCanhBao(nganSach);
            return nganSach;
        }

        public async Task<IEnumerable<TinhTrangNganSach>> LayCanhBao(int maNguoiDung, int thang, int nam)
        {
            var danhSach = await LayDanhSach(maNguoiDung, thang, nam, null);
            return danhSach.Where(x => x.DaChamNguong).ToList();
        }

        public async Task<IEnumerable<TinhTrangNganSach>> LayCanhBaoVuotNganSach(
            int maNguoiDung, int maDanhMuc, DateTime ngayGiaoDich)
        {
            var danhSach = await LayDanhSach(maNguoiDung, ngayGiaoDich.Month, ngayGiaoDich.Year, maDanhMuc);
            return danhSach.Where(x => x.DaVuotNganSach).ToList();
        }

        public Task<int> ThemNganSach(NganSach nganSach)
        {
            KiemTraNganSach(nganSach);
            return nganSachRepository.ThemNganSach(nganSach);
        }

        public Task<int> CapNhatNganSach(NganSach nganSach)
        {
            KiemTraNganSach(nganSach);
            return nganSachRepository.CapNhatNganSach(nganSach);
        }

        public Task<int> XoaMemNganSach(int maNganSach, int maNguoiDung)
        {
            return nganSachRepository.XoaMemNganSach(maNganSach, maNguoiDung);
        }

        private static void KiemTraThangNam(int? thang, int? nam)
        {
            if (thang.HasValue && (thang.Value < 1 || thang.Value > 12))
                throw new ArgumentException("Tháng phải từ 1 đến 12");
            if (nam.HasValue && (nam.Value < 2000 || nam.Value > 9998))
                throw new ArgumentException("Năm phải từ 2000 đến 9998");
        }

        private static void KiemTraNganSach(NganSach nganSach)
        {
            KiemTraThangNam(nganSach.Thang, nganSach.Nam);
            if (nganSach.MaDanhMuc <= 0)
                throw new ArgumentException("Mã danh mục không hợp lệ");
            if (nganSach.SoTienGioiHan <= 0 || nganSach.SoTienGioiHan > 9999999999999999.99m)
                throw new ArgumentException("Số tiền giới hạn phải lớn hơn 0 và nằm trong giới hạn cho phép");
            if (nganSach.SoTienGioiHan != decimal.Round(nganSach.SoTienGioiHan, 2))
                throw new ArgumentException("Số tiền giới hạn không được có quá 2 chữ số thập phân");
            if (nganSach.PhanTramCanhBao <= 0 || nganSach.PhanTramCanhBao > 100)
                throw new ArgumentException("Phần trăm cảnh báo phải lớn hơn 0 và không vượt quá 100");
            if (nganSach.PhanTramCanhBao != decimal.Round(nganSach.PhanTramCanhBao, 2))
                throw new ArgumentException("Phần trăm cảnh báo không được có quá 2 chữ số thập phân");
        }

        private static void TinhTienDoVaCanhBao(TinhTrangNganSach nganSach)
        {
            nganSach.SoTienConLai = nganSach.SoTienGioiHan - nganSach.TongChi;
            nganSach.PhanTramDaSuDung = decimal.Round(nganSach.TongChi / nganSach.SoTienGioiHan * 100, 2);
            // So sánh số tiền gốc để việc làm tròn phần trăm không gây cảnh báo sớm.
            nganSach.DaChamNguong = nganSach.TongChi >= nganSach.SoTienGioiHan * nganSach.PhanTramCanhBao / 100;
            nganSach.DaVuotNganSach = nganSach.TongChi > nganSach.SoTienGioiHan;
            nganSach.TrangThai = nganSach.DaVuotNganSach ? "VUOT_NGAN_SACH"
                : nganSach.TongChi == nganSach.SoTienGioiHan ? "DAT_GIOI_HAN"
                : nganSach.DaChamNguong ? "CHAM_NGUONG" : "BINH_THUONG";
            nganSach.ThongBaoCanhBao = nganSach.DaVuotNganSach
                ? "Chi tiêu đã vượt ngân sách của danh mục trong tháng này"
                : nganSach.TongChi == nganSach.SoTienGioiHan
                    ? "Chi tiêu đã đạt giới hạn ngân sách của danh mục trong tháng này"
                    : nganSach.DaChamNguong ? "Chi tiêu đã chạm ngưỡng cảnh báo ngân sách" : null;
        }
    }
}
