namespace EcoFashionBackEnd.Services
{
    public class OrderPayoutBackgroundService : BackgroundService
    {
        private readonly ILogger<OrderPayoutBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OrderPayoutBackgroundService(
            ILogger<OrderPayoutBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order payout service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var payoutService = scope.ServiceProvider.GetRequiredService<IOrderPayoutService>();

                    await payoutService.ProcessPayoutsAsync();

                    _logger.LogInformation("Order payout cycle executed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing order payouts");
                }

                // Sleep interval  1 min check 1  )
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Order payout service stopped.");
        }
    }

}
