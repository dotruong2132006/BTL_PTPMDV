using System.Text.Json.Serialization;

namespace QuanLyNganQuyCaNhan.DAL.Models
{
    public class BaoCaoThang
    {
        public long MaBaoCao { get; set; }
        public int MaNguoiDung { get; set; }
        public Guid MaYeuCau { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string TrangThai { get; set; } = "CHO_XU_LY";
        public int SoLanThu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayHoanThanh { get; set; }
        public string? LoiCuoi { get; set; }
        public long KichThuocFile { get; set; }
        [JsonIgnore]
        public Guid? MaLanXuLy { get; set; }
    }

    public class DuLieuBaoCaoThang
    {
        public string HoTen { get; set; } = "";
        public int Thang { get; set; }
        public int Nam { get; set; }
        public DateTime NgayLayDuLieu { get; set; }
        public List<GiaoDich> DanhSach { get; set; } = new();
    }
}
