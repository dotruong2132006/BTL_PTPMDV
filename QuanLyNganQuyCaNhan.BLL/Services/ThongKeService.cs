using Microsoft.Data.SqlClient;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class ThongKeService : IThongKeService
    {
        private readonly ThongKeRepository thongKeRepository;
        public ThongKeService(ThongKeRepository thongKeRepository)
        {
            this.thongKeRepository = thongKeRepository;
        }

        public async Task<TongQuanTaiChinh> LayTongQuan(int maNguoiDung, int thang, int nam, CancellationToken cancellationToken)
        {
            KiemTra(thang, nam, null);
            try { return await thongKeRepository.LayTongQuan(maNguoiDung, thang, nam, cancellationToken); }
            catch (SqlException ex) when (ex.Number == 50001) { throw new ArgumentException("Người dùng không còn hoạt động", ex); }
        }

        public async Task<IEnumerable<ThongKeThang>> LayTheoThang(int maNguoiDung, int nam, int? maVi, CancellationToken cancellationToken)
        {
            KiemTra(1, nam, maVi);
            try
            {
                var duLieu = (await thongKeRepository.LayTheoThang(maNguoiDung, nam, maVi, cancellationToken)).ToDictionary(x => x.Thang);
                return Enumerable.Range(1, 12).Select(thang => duLieu.TryGetValue(thang, out var item)
                    ? item : new ThongKeThang { Thang = thang, Nam = nam }).ToList();
            }
            catch (SqlException ex) when (ex.Number == 50001) { throw new ArgumentException("Người dùng không còn hoạt động", ex); }
        }

        public async Task<IEnumerable<ThongKeDanhMuc>> LayTheoDanhMuc(int maNguoiDung, int thang, int nam, int? maVi, CancellationToken cancellationToken)
        {
            KiemTra(thang, nam, maVi);
            try
            {
                var duLieu = (await thongKeRepository.LayTheoDanhMuc(maNguoiDung, thang, nam, maVi, cancellationToken)).ToList();
                foreach (var nhom in duLieu.GroupBy(x => x.LoaiGiaoDich))
                {
                    decimal tong = nhom.Sum(x => x.TongTien);
                    foreach (var item in nhom) item.TyLePhanTram = tong == 0 ? 0 : Math.Round(item.TongTien / tong * 100, 2);
                }
                return duLieu;
            }
            catch (SqlException ex) when (ex.Number == 50001) { throw new ArgumentException("Người dùng không còn hoạt động", ex); }
        }

        private static void KiemTra(int thang, int nam, int? maVi)
        {
            if (thang < 1 || thang > 12 || nam < 2000 || nam > 9998) throw new ArgumentException("Tháng từ 1–12, năm từ 2000–9998");
            if (maVi.HasValue && maVi <= 0) throw new ArgumentException("Mã ví phải lớn hơn 0");
        }
    }
}
