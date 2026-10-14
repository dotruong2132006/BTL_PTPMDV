using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QuanLyNganQuyCaNhan.API.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService healthCheckService;
        public HealthController(HealthCheckService healthCheckService) { this.healthCheckService = healthCheckService; }

        [HttpGet]
        public async Task<IActionResult> KiemTra()
        {
            var ketQua = await healthCheckService.CheckHealthAsync(HttpContext.RequestAborted);
            Response.Headers.CacheControl = "no-store";
            return StatusCode(ketQua.Status == HealthStatus.Healthy ? 200 : 503,
                new { TrangThai = ketQua.Status.ToString(),
                    ThanhPhan = ketQua.Entries.Select(x => new { Ten = x.Key,
                        TrangThai = x.Value.Status.ToString(), MoTa = x.Value.Description }) });
        }
    }
}
