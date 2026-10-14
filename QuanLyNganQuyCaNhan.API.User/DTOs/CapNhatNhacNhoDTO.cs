using System.ComponentModel.DataAnnotations;

namespace QuanLyNganQuyCaNhan.API.User.DTOs
{
    public class CapNhatNhacNhoDTO : ThemNhacNhoDTO
    {
        [Range(1, int.MaxValue)]
        public int PhienBan { get; set; }
    }
}
