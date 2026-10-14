namespace QuanLyNganQuyCaNhan.API.User.DTOs
{
    public class NhapFileGiaoDichDTO
    {
        public IFormFile File { get; set; } = null!;
        public Guid MaYeuCau { get; set; }
    }
}
