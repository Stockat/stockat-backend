using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Stockat.API.ActionFilters;
using Stockat.API.Extensions;
using Stockat.Core.Entities;
using Stockat.Core.IServices;
using Stockat.API.ActionFilters;
using Stockat.Core.Entities;
using Stockat.API.Hubs;
using Stockat.Service.Services.AuctionServices;
using System.Text.Json.Serialization;

namespace Stockat.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
        "/nlog.config"));
        builder.Services.AddSwaggerDocumentation();
        // Add services to the container.
        builder.Services.ConfigureCors();
        builder.Services.ConfigureIISIntegration();

        builder.Services.ConfigureLoggerService(); // register logger service
        builder.Services.ConfigureSqlContext(builder.Configuration); // register db context
        builder.Services.ConfigureIdentity(); // register identity
        builder.Services.ConfigureJWT(builder.Configuration);

        builder.Services.ConfigureRepositoryManager(); // adding ef (infra) layer dependencies
        builder.Services.ConfigureServiceManager(); // adding service layer dependencies

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            // look at page 103 & 104 to understand why
            options.SuppressModelStateInvalidFilter = true; // it prevents us from sending our custom responses with different messages and status codes to the client. This will be very important once we get to the Validation on our entities
        });

        builder.Services.AddScoped<ValidationFilterAttribute>(); // custom validation
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // only use IHttpContextAccessor when necessary like accessing user claims, IP address, headers
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSignalR();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        //builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Stockat.Service")));

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


        //BackGround service injection
        builder.Services.AddHostedService<AuctionMonitorService>();

        var app = builder.Build();

        var logger = app.Services.GetRequiredService<ILoggerManager>();
        app.ConfigureExceptionHandler(logger);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerDocumentation();

        }
        else
        {
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All
        });


        app.UseCors("CorsPolicy");
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<ChatHub>("/chathub");
        app.MapControllers();

        app.Run();
    }
}
