using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stockat.Core;
using Stockat.Core.IServices.IAuctionServices;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stockat.API.Services;

//public class AuctionMonitorService : BackgroundService
//{
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly ILogger<AuctionMonitorService> _logger;

//    public AuctionMonitorService(IServiceScopeFactory scopeFactory, ILogger<AuctionMonitorService> logger)
//    {
//        _scopeFactory = scopeFactory;
//        _logger = logger;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();
//                var notificationService = scope.ServiceProvider.GetRequiredService<IAuctionNotificationService>();

//                try
//                {
//                    // Close ended auctions and send notifications as needed
//                    await serviceManager.AuctionService.CloseEndedAuctionsAsync();
//                    // You can add more logic here for starting auctions, sending 'ending soon' notifications, etc.
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error in AuctionMonitorService background task");
//                }
//            }

//            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every minute
//        }
//    }
//}