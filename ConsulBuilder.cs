using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Consul;
using System;

namespace ConsulServiceDiscovery
{
    public static class ConsulBuilder
    {
        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            if (!configuration.GetSection("consulConfig").Exists())
                return services;

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ConsulHostedService>();
            services.Configure<ConsulConfig>(configuration.GetSection("consulConfig"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration["consulConfig:address"];
                consulConfig.Address = new Uri(address);
            }));

            return services;
        }

        public static async void UnregisterFromConsul(IConfiguration configuration)
        {
            if (!configuration.GetSection("consulConfig").Exists())
                return;

            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(configuration["consulConfig:address"]);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}
