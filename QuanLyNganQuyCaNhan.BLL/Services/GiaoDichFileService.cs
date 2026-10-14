using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class GiaoDichFileService
    {
        public const int GioiHanDungLuong = 5 * 1024 * 1024;
        private static readonly string[] TieuDe = { "MaVi", "MaDanhMuc", "LoaiGiaoDich", "SoTien", "NgayGiaoDich", "NoiDung" };
        private readonly GiaoDichRepository giaoDichRepository;
        public GiaoDichFileService(GiaoDichRepository giaoDichRepository)
        {
            this.giaoDichRepository = giaoDichRepository;
        }

        public async Task<KetQuaNhapGiaoDich> NhapFile(int maNguoiDung, Guid maYeuCau,
            string tenFile, byte[] noiDung, string? diaChiIP)
        {
            if (maYeuCau == Guid.Empty) throw new ArgumentException("Mã yêu cầu nhập phải là GUID khác rỗng");
            if (noiDung.Length == 0 || noiDung.Length > GioiHanDungLuong)
                throw new ArgumentException("File phải có dữ liệu và không vượt quá 5 MB");
            string dinhDang = Path.GetExtension(tenFile).ToLowerInvariant();
            if (dinhDang != ".csv" && dinhDang != ".xlsx")
                throw new ArgumentException("Chỉ hỗ trợ file .csv UTF-8 và Excel .xlsx");
            List<GiaoDich> danhSach;
            try
            {
                danhSach = dinhDang == ".csv" ? await DocCSV(noiDung, maNguoiDung) : DocExcel(noiDung, maNguoiDung);
            }
            catch (CsvHelperException ex) { throw new ArgumentException("File CSV sai cấu trúc dấu phân cách hoặc dấu ngoặc kép", ex); }
            catch (DecoderFallbackException ex) { throw new ArgumentException("File CSV phải dùng mã hóa UTF-8", ex); }
            catch (InvalidDataException ex) { throw new ArgumentException("File Excel không hợp lệ hoặc bị hỏng", ex); }
            catch (FormatException ex) { throw new ArgumentException("File chứa giá trị sai định dạng", ex); }
            catch (Exception ex) when (dinhDang == ".xlsx" && (ex is System.Xml.XmlException
                || ex is DocumentFormat.OpenXml.Packaging.OpenXmlPackageException || ex is InvalidOperationException))
            { throw new ArgumentException("File Excel không hợp lệ hoặc bị hỏng", ex); }
            if (danhSach.Count == 0) throw new ArgumentException("File không có dòng giao dịch");
            for (int i = 0; i < danhSach.Count; i++)
            {
                try { GiaoDichService.KiemTraDuLieuGiaoDich(danhSach[i]); }
                catch (ArgumentException ex) { throw new ArgumentException($"Bản ghi {i + 1}: {ex.Message}", ex); }
            }
            // Kèm định dạng vào fingerprint để GUID retry không bị dùng với nội dung khác.
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(dinhDang).Concat(noiDung).ToArray());
            return await giaoDichRepository.NhapGiaoDich(maNguoiDung, maYeuCau, hash, danhSach, diaChiIP);
        }

        public async Task<byte[]> XuatFile(int maNguoiDung, string dinhDang, DateTime? tuNgay, DateTime? denNgay,
            int? maDanhMuc, int? maVi)
        {
            dinhDang = KiemTraDinhDang(dinhDang);
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
                throw new ArgumentException("Từ ngày không được sau đến ngày");
            if (denNgay.HasValue && denNgay.Value.Year > 9998) throw new ArgumentException("Đến ngày phải trước năm 9999");
            if ((maDanhMuc.HasValue && maDanhMuc <= 0) || (maVi.HasValue && maVi <= 0))
                throw new ArgumentException("Mã danh mục và mã ví phải lớn hơn 0");
            var danhSach = (await giaoDichRepository.LayDuLieuXuat(maNguoiDung, tuNgay?.Date,
                denNgay?.Date.AddDays(1), maDanhMuc, maVi)).ToList();
            if (danhSach.Count > 10000) throw new ArgumentException("Tối đa 10.000 giao dịch mỗi file xuất; hãy thu hẹp bộ lọc");
            return dinhDang == "csv" ? await TaoCSV(danhSach) : TaoExcel(danhSach);
        }

        public Task<byte[]> TaoFileMau(string dinhDang)
        {
            return KiemTraDinhDang(dinhDang) == "csv" ? TaoCSV(Array.Empty<GiaoDich>()) : Task.FromResult(TaoExcel(Array.Empty<GiaoDich>()));
        }

        private static string KiemTraDinhDang(string dinhDang)
        {
            dinhDang = (dinhDang ?? "").Trim().ToLowerInvariant();
            if (dinhDang != "csv" && dinhDang != "xlsx") throw new ArgumentException("Định dạng phải là csv hoặc xlsx");
            return dinhDang;
        }

        private static async Task<List<GiaoDich>> DocCSV(byte[] noiDung, int maNguoiDung)
        {
            using var stream = new MemoryStream(noiDung);
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",", IgnoreBlankLines = true
            });
            if (!await csv.ReadAsync()) throw new ArgumentException("File CSV không có tiêu đề");
            csv.ReadHeader();
            KiemTraTieuDe(csv.HeaderRecord ?? Array.Empty<string>());
            var ketQua = new List<GiaoDich>();
            while (await csv.ReadAsync())
            {
                if (ketQua.Count >= 1000) throw new ArgumentException("Tối đa 1.000 giao dịch mỗi lần nhập");
                if (csv.Parser.Count != TieuDe.Length) throw new ArgumentException($"Dòng {csv.Parser.RawRow}: cần đúng 6 cột");
                ketQua.Add(ChuyenThanhGiaoDich(Enumerable.Range(0, 6).Select(x => csv.GetField(x) ?? "").ToArray(), maNguoiDung, csv.Parser.RawRow));
            }
            return ketQua;
        }

        private static List<GiaoDich> DocExcel(byte[] noiDung, int maNguoiDung)
        {
            using var stream = new MemoryStream(noiDung);
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                if (zip.Entries.Count > 10000 || zip.Entries.Sum(x => x.Length) > 50L * 1024 * 1024)
                    throw new ArgumentException("File Excel có dung lượng giải nén quá lớn");
                if (!zip.Entries.Any(x => x.FullName == "xl/workbook.xml")) throw new ArgumentException("File không phải Excel .xlsx hợp lệ");
            }
            stream.Position = 0;
            using var workbook = new XLWorkbook(stream);
            if (workbook.Worksheets.Count != 1) throw new ArgumentException("File Excel phải có đúng một sheet GiaoDich");
            var sheet = workbook.Worksheets.First();
            if (sheet.Name != "GiaoDich") throw new ArgumentException("Tên sheet phải là GiaoDich");
            int soDong = sheet.LastRowUsed(XLCellsUsedOptions.Contents)?.RowNumber() ?? 0;
            int soCot = sheet.LastColumnUsed(XLCellsUsedOptions.Contents)?.ColumnNumber() ?? 0;
            if (soDong > 1001 || soCot != 6) throw new ArgumentException("Excel cần đúng 6 cột và tối đa 1.000 dòng dữ liệu");
            if (sheet.Row(1).Cells(1, 6).Any(x => x.HasFormula)) throw new ArgumentException("Tiêu đề không được chứa công thức");
            KiemTraTieuDe(Enumerable.Range(1, 6).Select(x => sheet.Cell(1, x).GetString()).ToArray());
            var ketQua = new List<GiaoDich>();
            for (int dong = 2; dong <= soDong; dong++)
            {
                var cacO = sheet.Row(dong).Cells(1, 6).ToArray();
                if (cacO.All(x => x.IsEmpty())) continue;
                if (cacO.Any(x => x.HasFormula)) throw new ArgumentException($"Dòng {dong}: không nhận công thức Excel");
                var giaTri = cacO.Select(x => x.DataType == XLDataType.Number
                    ? x.GetDouble().ToString("R", CultureInfo.InvariantCulture)
                    : x.DataType == XLDataType.DateTime ? x.GetDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture)
                    : x.GetString()).ToArray();
                ketQua.Add(ChuyenThanhGiaoDich(giaTri, maNguoiDung, dong));
            }
            return ketQua;
        }

        private static void KiemTraTieuDe(string[] tieuDe)
        {
            if (!tieuDe.Select(x => x.Trim()).SequenceEqual(TieuDe, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Tiêu đề và thứ tự cột phải là: " + string.Join(",", TieuDe));
        }

        private static GiaoDich ChuyenThanhGiaoDich(string[] giaTri, int maNguoiDung, int dong)
        {
            if (!int.TryParse(giaTri[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int maVi)
                || !int.TryParse(giaTri[1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int maDanhMuc))
                throw new ArgumentException($"Dòng {dong}: mã ví/danh mục phải là số nguyên dương");
            if (!decimal.TryParse(giaTri[3].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal soTien))
                throw new ArgumentException($"Dòng {dong}: số tiền dùng dấu chấm thập phân, không có phân cách hàng nghìn");
            string[] cacDinhDang = { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF", "yyyy-MM-dd HH:mm:ss" };
            if (!DateTime.TryParseExact(giaTri[4].Trim(), cacDinhDang, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngay))
                throw new ArgumentException($"Dòng {dong}: ngày dùng yyyy-MM-dd hoặc yyyy-MM-ddTHH:mm:ss");
            return new GiaoDich { MaNguoiDung = maNguoiDung, MaVi = maVi, MaDanhMuc = maDanhMuc,
                LoaiGiaoDich = giaTri[2], SoTien = soTien, NgayGiaoDich = ngay, NoiDung = giaTri[5] };
        }

        private static async Task<byte[]> TaoCSV(IEnumerable<GiaoDich> danhSach)
        {
            using var stream = new MemoryStream();
            await using (var writer = new StreamWriter(stream, new UTF8Encoding(true), 1024, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (string ten in TieuDe) csv.WriteField(ten);
                await csv.NextRecordAsync();
                foreach (var gd in danhSach)
                {
                    csv.WriteField(gd.MaVi);
                    csv.WriteField(gd.MaDanhMuc);
                    csv.WriteField(gd.LoaiGiaoDich);
                    csv.WriteField(gd.SoTien.ToString("0.00", CultureInfo.InvariantCulture));
                    csv.WriteField(gd.NgayGiaoDich.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture));
                    string noiDung = gd.NoiDung ?? "";
                    string kiemTra = noiDung.TrimStart();
                    if ((kiemTra.Length > 0 && "=+-@".Contains(kiemTra[0]))
                        || (noiDung.Length > 0 && "\t\r\n".Contains(noiDung[0]))) noiDung = "'" + noiDung;
                    csv.WriteField(noiDung);
                    await csv.NextRecordAsync();
                }
                await writer.FlushAsync();
            }
            return stream.ToArray();
        }

        private static byte[] TaoExcel(IEnumerable<GiaoDich> danhSach)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("GiaoDich");
            for (int cot = 0; cot < TieuDe.Length; cot++) sheet.Cell(1, cot + 1).Value = TieuDe[cot];
            int dong = 2;
            foreach (var gd in danhSach)
            {
                sheet.Cell(dong, 1).Value = gd.MaVi;
                sheet.Cell(dong, 2).Value = gd.MaDanhMuc;
                sheet.Cell(dong, 3).Value = gd.LoaiGiaoDich;
                // Tiền dưới dạng text để Excel không mất chính xác sau 15 chữ số.
                sheet.Cell(dong, 4).Value = gd.SoTien.ToString("0.00", CultureInfo.InvariantCulture);
                sheet.Cell(dong, 5).Value = gd.NgayGiaoDich.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                sheet.Cell(dong, 6).Value = gd.NoiDung ?? "";
                dong++;
            }
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            sheet.SheetView.FreezeRows(1);
            sheet.Columns(1, 3).Width = 18;
            sheet.Column(4).Width = 25;
            sheet.Column(5).Width = 32;
            sheet.Column(6).Width = 50;
            sheet.Column(6).Style.Alignment.WrapText = true;
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
