using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace QuanLyNganQuyCaNhan.API.Shared.Extensions
{
    public static class ApiGatewayExtensions
    {
        public static IApplicationBuilder SuDungIpTuApiGateway(this IApplicationBuilder app, IConfiguration cauHinh)
        {
            var tuyChon = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor,
                ForwardLimit = 1
            };
            tuyChon.KnownNetworks.Clear();
            tuyChon.KnownProxies.Clear();

            var proxyTinCay = cauHinh.GetSection("ApiGateway:ProxyTinCay").GetChildren()
                .Select(x => x.Value).ToArray();
            if (proxyTinCay.Length == 0) proxyTinCay = new[] { "127.0.0.1", "::1" };

            foreach (var diaChi in proxyTinCay)
            {
                if (!IPAddress.TryParse(diaChi, out var ip))
                    throw new InvalidOperationException("ApiGateway:ProxyTinCay phải chứa địa chỉ IP hợp lệ");
                tuyChon.KnownProxies.Add(ip);
            }

            // Chỉ tin IP chuyển tiếp từ Gateway đã cấu hình, phục vụ audit và rate limit.
            return app.UseForwardedHeaders(tuyChon);
        }
    }
}
