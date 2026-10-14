using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QuanLyNganQuyCaNhan.DAL.Connections;

namespace QuanLyNganQuyCaNhan.API.Shared.HealthChecks
{
    public class KetNoiCSDLHealthCheck : IHealthCheck
    {
        private readonly DapperConnectionFactory ketNoiCSDL;
        public KetNoiCSDLHealthCheck(DapperConnectionFactory ketNoiCSDL) { this.ketNoiCSDL = ketNoiCSDL; }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var ketNoi = ketNoiCSDL.TaoKetNoi();
                await ketNoi.OpenAsync(cancellationToken);
                await ketNoi.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", commandTimeout: 5, cancellationToken: cancellationToken));
                return HealthCheckResult.Healthy("Kết nối database hoạt động");
            }
            catch (Exception) { return HealthCheckResult.Unhealthy("Không kết nối được database"); }
        }
    }
}
