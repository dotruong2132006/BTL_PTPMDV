using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface IThongKeService
    {
        Task<TongQuanTaiChinh> LayTongQuan(int maNguoiDung, int thang, int nam, CancellationToken cancellationToken);
        Task<IEnumerable<ThongKeThang>> LayTheoThang(int maNguoiDung, int nam, int? maVi, CancellationToken cancellationToken);
        Task<IEnumerable<ThongKeDanhMuc>> LayTheoDanhMuc(int maNguoiDung, int thang, int nam, int? maVi, CancellationToken cancellationToken);
    }
}
