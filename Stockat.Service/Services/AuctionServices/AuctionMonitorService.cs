using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stockat.Core;
using Stockat.Core.IServices;

namespace Stockat.Service.Services.AuctionServices
{
    public class AuctionMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AuctionMonitorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();

                    await serviceManager.AuctionService.CloseEndedAuctionsAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }

}
