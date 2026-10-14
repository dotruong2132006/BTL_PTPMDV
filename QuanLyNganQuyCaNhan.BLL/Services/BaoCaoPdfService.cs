using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.Globalization;
using System.Text;
using QuanLyNganQuyCaNhan.DAL.Models;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class BaoCaoPdfService
    {
        private static readonly CultureInfo VanHoa = CultureInfo.GetCultureInfo("vi-VN");
        private static readonly object KhoaTaoFont = new();
        private static readonly Color MauTieuDe = Color.FromRgb(29, 65, 92);

        public byte[] TaoFile(DuLieuBaoCaoThang duLieu)
        {
            lock (KhoaTaoFont)
            {
                // Dùng Unicode cho mọi đoạn chữ, tránh ánh xạ WinAnsi làm mất dấu tiếng Việt.
                GlobalFontSettings.DefaultFontEncoding = PdfFontEncoding.Unicode;
                if (GlobalFontSettings.FontResolver == null) GlobalFontSettings.FontResolver = new BaoCaoFontResolver();
            }
            var taiLieu = new Document();
            taiLieu.Info.Title = $"Báo cáo tài chính tháng {duLieu.Thang:00}/{duLieu.Nam}";
            taiLieu.Info.Author = "Quản lý ngân quỹ cá nhân";
            taiLieu.Styles[StyleNames.Normal]!.Font.Name = "Noto Sans";
            taiLieu.Styles[StyleNames.Normal]!.Font.Size = 9;
            taiLieu.Styles[StyleNames.Normal]!.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);
            taiLieu.Styles[StyleNames.Heading1]!.Font.Name = "Noto Sans";
            taiLieu.Styles[StyleNames.Heading1]!.Font.Size = 13;
            taiLieu.Styles[StyleNames.Heading1]!.Font.Bold = true;
            taiLieu.Styles[StyleNames.Heading1]!.Font.Color = MauTieuDe;
            var trang = taiLieu.AddSection();
            trang.PageSetup.PageFormat = PageFormat.A4;
            trang.PageSetup.Orientation = Orientation.Landscape;
            trang.PageSetup.LeftMargin = trang.PageSetup.RightMargin = Unit.FromCentimeter(1);
            trang.PageSetup.TopMargin = trang.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            var chanTrang = trang.Footers.Primary.AddParagraph("Quản lý ngân quỹ cá nhân | Trang ");
            chanTrang.AddPageField();
            chanTrang.AddText(" / ");
            chanTrang.AddNumPagesField();
            chanTrang.Format.Alignment = ParagraphAlignment.Right;
            chanTrang.Format.Font.Size = 8;

            var tieuDe = trang.AddParagraph($"BÁO CÁO TÀI CHÍNH THÁNG {duLieu.Thang:00}/{duLieu.Nam}");
            tieuDe.Format.Font.Size = 20;
            tieuDe.Format.Font.Bold = true;
            tieuDe.Format.Font.Color = MauTieuDe;
            trang.AddParagraph("Người dùng: " + VanBan(duLieu.HoTen));
            trang.AddParagraph($"Thời điểm lấy dữ liệu: {duLieu.NgayLayDuLieu.AddHours(7):dd/MM/yyyy HH:mm:ss} (UTC+7)");
            decimal tongThu = duLieu.DanhSach.Where(x => x.LoaiGiaoDich == "THU").Sum(x => x.SoTien);
            decimal tongChi = duLieu.DanhSach.Where(x => x.LoaiGiaoDich == "CHI").Sum(x => x.SoTien);
            trang.AddParagraph("TỔNG QUAN", StyleNames.Heading1);
            trang.AddParagraph($"Tổng thu: {Tien(tongThu)} VND    Tổng chi: {Tien(tongChi)} VND");
            trang.AddParagraph($"Chênh lệch thu/chi: {Tien(tongThu - tongChi)} VND    Số giao dịch: {duLieu.DanhSach.Count:N0}");
            trang.AddParagraph("Chênh lệch thu/chi là biến động từ các giao dịch trong tháng, không phải số dư ví. Báo cáo chỉ tính giao dịch chưa xóa.");

            if (duLieu.DanhSach.Count == 0)
            {
                trang.AddParagraph("Tháng này chưa có giao dịch.");
            }
            else
            {
                trang.AddParagraph("THỐNG KÊ THEO DANH MỤC", StyleNames.Heading1);
                var thongKe = TaoBang(trang, new[] { 11.0, 3.0, 5.0, 8.7 }, new[] { "Danh mục", "Loại", "Số giao dịch", "Tổng tiền (VND)" });
                foreach (var nhom in duLieu.DanhSach.GroupBy(x => new { x.MaDanhMuc, x.TenDanhMuc, x.LoaiGiaoDich })
                    .OrderBy(x => x.Key.LoaiGiaoDich).ThenByDescending(x => x.Sum(gd => gd.SoTien)))
                {
                    var dong = thongKe.AddRow();
                    ThemO(dong, 0, nhom.Key.TenDanhMuc ?? "Danh mục không còn khả dụng");
                    ThemO(dong, 1, nhom.Key.LoaiGiaoDich);
                    ThemO(dong, 2, nhom.Count().ToString(VanHoa), true);
                    ThemO(dong, 3, Tien(nhom.Sum(x => x.SoTien)), true);
                }

                trang.AddPageBreak();
                trang.AddParagraph("CHI TIẾT GIAO DỊCH", StyleNames.Heading1);
                var chiTiet = TaoBang(trang, new[] { 3.3, 1.0, 3.3, 3.8, 3.8, 12.5 },
                    new[] { "Ngày", "Loại", "Danh mục", "Ví", "Số tiền (VND)", "Nội dung" });
                foreach (var gd in duLieu.DanhSach)
                {
                    var dong = chiTiet.AddRow();
                    ThemO(dong, 0, gd.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm", VanHoa));
                    ThemO(dong, 1, gd.LoaiGiaoDich);
                    ThemO(dong, 2, gd.TenDanhMuc ?? "Không còn khả dụng");
                    ThemO(dong, 3, gd.TenVi ?? "Không còn khả dụng");
                    ThemO(dong, 4, Tien(gd.SoTien), true);
                    ThemO(dong, 5, gd.NoiDung ?? "");
                }
            }
            var renderer = new PdfDocumentRenderer { Document = taiLieu };
            renderer.RenderDocument();
            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }

        private static Table TaoBang(Section trang, double[] doRong, string[] tieuDe)
        {
            var bang = trang.AddTable();
            bang.Borders.Width = Unit.FromPoint(0.4);
            bang.Borders.Color = Colors.LightGray;
            bang.Format.Font.Size = 8;
            bang.Format.SpaceAfter = Unit.FromPoint(2);
            bang.TopPadding = bang.BottomPadding = Unit.FromPoint(4);
            foreach (double rong in doRong) bang.AddColumn(Unit.FromCentimeter(rong));
            var dong = bang.AddRow();
            dong.HeadingFormat = true;
            dong.Shading.Color = MauTieuDe;
            dong.Format.Font.Bold = true;
            dong.Format.Font.Color = Colors.White;
            for (int i = 0; i < tieuDe.Length; i++) dong.Cells[i].AddParagraph(tieuDe[i]);
            return bang;
        }

        private static void ThemO(Row dong, int cot, string noiDung, bool canPhai = false)
        {
            var doan = dong.Cells[cot].AddParagraph(VanBan(noiDung));
            if (canPhai) doan.Format.Alignment = ParagraphAlignment.Right;
        }

        private static string Tien(decimal giaTri) => giaTri.ToString("N2", VanHoa);
        private static string VanBan(string giaTri) => new string(giaTri.Normalize(NormalizationForm.FormC)
            .Where(x => !char.IsControl(x) || x == '\n').ToArray());

        private sealed class BaoCaoFontResolver : IFontResolver
        {
            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                return new FontResolverInfo(isBold ? "NotoSans-Bold" : "NotoSans-Regular", false, isItalic);
            }

            public byte[] GetFont(string faceName)
            {
                using var stream = typeof(BaoCaoPdfService).Assembly.GetManifestResourceStream($"QuanLyNganQuyCaNhan.BLL.Fonts.{faceName}.ttf")
                    ?? throw new InvalidOperationException("Thiếu font tiếng Việt để tạo PDF");
                using var boNho = new MemoryStream();
                stream.CopyTo(boNho);
                return boNho.ToArray();
            }
        }
    }
}
