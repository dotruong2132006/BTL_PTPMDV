using QuanLyNganQuyCaNhan.BLL.Interfaces;

namespace QuanLyNganQuyCaNhan.API.User.Jobs
{
    public class NhacNhoBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<NhacNhoBackgroundService> logger;

        public NhacNhoBackgroundService(IServiceScopeFactory scopeFactory, ILogger<NhacNhoBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            logger.LogInformation("Worker nhắc nhở đã chạy; kiểm tra lịch mỗi phút");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await using var scope = scopeFactory.CreateAsyncScope();
                        var nhacNhoService = scope.ServiceProvider.GetRequiredService<INhacNhoService>();
                        await nhacNhoService.XuLyDenHan();
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Xử lý nhắc nhở thất bại; sẽ thử lại ở lượt quét sau");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            logger.LogInformation("Worker nhắc nhở đã dừng");
        }
    }
}
