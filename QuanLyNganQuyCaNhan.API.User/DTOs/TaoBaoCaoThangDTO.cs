using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.API.User.DTOs
{
    public class TaoBaoCaoThangDTO
    {
        [Range(1, 12)]
        public int Thang { get; set; }
        [Range(2000, 9998)]
        public int Nam { get; set; }
        public Guid MaYeuCau { get; set; }
    }
}
