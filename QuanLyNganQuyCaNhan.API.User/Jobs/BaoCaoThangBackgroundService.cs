using QuanLyNganQuyCaNhan.BLL.Interfaces;

namespace QuanLyNganQuyCaNhan.API.User.Jobs
{
    public class BaoCaoThangBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<BaoCaoThangBackgroundService> logger;
        public BaoCaoThangBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BaoCaoThangBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            logger.LogInformation("Worker báo cáo PDF đã chạy");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    bool coYeuCau = false;
                    try
                    {
                        await using var scope = scopeFactory.CreateAsyncScope();
                        coYeuCau = await scope.ServiceProvider.GetRequiredService<IBaoCaoThangService>().XuLyHangDoi(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                    catch (Exception ex) { logger.LogError(ex, "Lỗi hàng đợi báo cáo PDF; sẽ thử lại sau 5 giây"); }
                    if (!coYeuCau) await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }
    }
}
