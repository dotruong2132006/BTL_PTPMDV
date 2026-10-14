using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuanLyNganQuyCaNhan.DAL.Models;
using QuanLyNganQuyCaNhan.DAL.Repositories;

namespace QuanLyNganQuyCaNhan.API.Shared.Middleware
{
    public class AuditLogMiddleware : IMiddleware
    {
        private readonly NhatKyAuditRepository nhatKyAuditRepository;
        private readonly ILogger<AuditLogMiddleware> logger;

        private static readonly Dictionary<string, string> TenKhoaTheoBang = new(StringComparer.OrdinalIgnoreCase)
        {
            ["GiaoDich"] = "maGiaoDich",
            ["DanhMuc"] = "maDanhMuc",
            ["Vi"] = "maVi",
            ["NganSach"] = "maNganSach",
            ["NhacNho"] = "maNhacNho",
            ["NguoiDung"] = "maNguoiDung"
        };

        public AuditLogMiddleware(
            NhatKyAuditRepository nhatKyAuditRepository,
            ILogger<AuditLogMiddleware> logger)
        {
            this.nhatKyAuditRepository = nhatKyAuditRepository;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Khôi phục mật khẩu ghi audit trong transaction nghiệp vụ; không đọc/log bộ mã hoặc OTP.
            if (LayTenController(context) == "QuenMatKhau")
            {
                await next(context);
                return;
            }
            string? tenBang = LayTenBang(context);
            string? hanhDong = LayHanhDong(context);
            if (tenBang == null || hanhDong == null)
            {
                await next(context);
                return;
            }

            int? maNguoiDung = LayMaNguoiDung(context);
            long? maBanGhi = LayMaBanGhiTuRoute(context, tenBang);
            string? tenController = LayTenController(context);
            if (!maBanGhi.HasValue && tenController == "HaiFa")
            {
                maBanGhi = maNguoiDung;
            }
            string? duLieuCu = null;

            if (maBanGhi.HasValue)
            {
                try
                {
                    duLieuCu = await nhatKyAuditRepository.LayDuLieuHienTai(tenBang, maBanGhi.Value, maNguoiDung);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Không đọc được dữ liệu cũ để ghi Audit Log cho {TenBang} {MaBanGhi}", tenBang, maBanGhi);
                }
            }

            Stream thanPhanHoiBanDau = context.Response.Body;
            await using var thanPhanHoiTam = new MemoryStream();
            context.Response.Body = thanPhanHoiTam;
            bool xuLyHoanTat = false;

            try
            {
                await next(context);
                xuLyHoanTat = true;
            }
            finally
            {
                context.Response.Body = thanPhanHoiBanDau;
                thanPhanHoiTam.Position = 0;
                string noiDungPhanHoi = await new StreamReader(
                    thanPhanHoiTam,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 1024,
                    leaveOpen: true).ReadToEndAsync();
                    thanPhanHoiTam.Position = 0;

                if (xuLyHoanTat && context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    try
                    {
                        maBanGhi ??= LayMaBanGhiTuPhanHoi(noiDungPhanHoi, tenBang);
                        string? duLieuMoi = null;
                        if (maBanGhi.HasValue)
                        {
                            duLieuMoi = await nhatKyAuditRepository.LayDuLieuHienTai(tenBang, maBanGhi.Value, maNguoiDung);
                        }

                        await nhatKyAuditRepository.ThemNhatKy(new NhatKyAudit
                        {
                            MaNguoiDung = maNguoiDung,
                            HanhDong = hanhDong,
                            TenBang = tenBang,
                            MaBanGhi = maBanGhi?.ToString(CultureInfo.InvariantCulture),
                            DuLieuCu = duLieuCu,
                            DuLieuMoi = duLieuMoi,
                            DiaChiIP = context.Connection.RemoteIpAddress?.ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Không ghi được Audit Log cho {TenBang} {MaBanGhi}", tenBang, maBanGhi);
                    }
                }

                await thanPhanHoiTam.CopyToAsync(thanPhanHoiBanDau);
            }
        }

        private static string? LayTenBang(HttpContext context)
        {
            string? tenController = LayTenController(context);
            string? tenAction = LayTenAction(context);

            if (tenController == "DangKy") return "NguoiDung";
            if (tenController == "DanhMucHeThong") return "DanhMuc";
            if (tenController == "HaiFa" && (tenAction == "Bat2FA" || tenAction == "Tat2FA"))
                return "NguoiDung";
            return tenController != null && TenKhoaTheoBang.ContainsKey(tenController)? tenController: null;
        }

        private static string? LayHanhDong(HttpContext context)
        {
            string? tenController = LayTenController(context);
            string? tenAction = LayTenAction(context);
            if (tenController == "HaiFa") return "CAP_NHAT_2FA";

            string phuongThuc = context.Request.Method;
            if (HttpMethods.IsPost(phuongThuc)) return "THEM";
            if (HttpMethods.IsPut(phuongThuc) || HttpMethods.IsPatch(phuongThuc)) return "CAP_NHAT";
            if (HttpMethods.IsDelete(phuongThuc)) return "XOA_MEM";
            return null;
        }
        private static string? LayTenController(HttpContext context)
        {
            return context.Request.RouteValues.TryGetValue("controller", out object? value)? Convert.ToString(value, CultureInfo.InvariantCulture): null;
        }

        private static string? LayTenAction(HttpContext context)
        {
            return context.Request.RouteValues.TryGetValue("action", out object? value)? Convert.ToString(value, CultureInfo.InvariantCulture): null;
        }

        private static int? LayMaNguoiDung(HttpContext context)
        {
            string? giaTri = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(giaTri, out int maNguoiDung) ? maNguoiDung : null;
        }

        private static long? LayMaBanGhiTuRoute(HttpContext context, string tenBang)
        {
            if (!TenKhoaTheoBang.TryGetValue(tenBang, out string? tenKhoa))
                return null;
            string? giaTri = context.Request.RouteValues.TryGetValue(tenKhoa, out object? value)? Convert.ToString(value, CultureInfo.InvariantCulture): null;
            return long.TryParse(giaTri, NumberStyles.None, CultureInfo.InvariantCulture, out long maBanGhi)? maBanGhi: null;
        }

        private static long? LayMaBanGhiTuPhanHoi(string noiDungPhanHoi, string tenBang)
        {
            try
            {
                using var taiLieu = JsonDocument.Parse(noiDungPhanHoi);
                string tenThuocTinh = "Ma" + tenBang;
                foreach (JsonProperty thuocTinh in taiLieu.RootElement.EnumerateObject())
                {
                    if (thuocTinh.Name.Equals(tenThuocTinh, StringComparison.OrdinalIgnoreCase)
                        && thuocTinh.Value.TryGetInt64(out long maBanGhi))
                    {
                        return maBanGhi;
                    }
                }
            }
            catch (JsonException)
            {
            }

            return null;
        }
    }
}
