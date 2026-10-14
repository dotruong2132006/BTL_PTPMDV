using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Interfaces
{
    public interface INhatKyAuditService
    {
        Task<(IEnumerable<NhatKyAudit> DanhSach, long TongSoBanGhi)> LayDanhSach(BoLocNhatKyAudit boLoc);
        Task<NhatKyAudit?> LayTheoMa(long maAudit);
    }
}
