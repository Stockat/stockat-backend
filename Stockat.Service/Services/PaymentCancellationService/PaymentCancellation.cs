using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stockat.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services.PaymentCancellationService;

public class PaymentCancellation : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentCancellation(IServiceScopeFactory scopeFactory)
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

                await serviceManager.OrderService.PaymentCancellation();


            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
