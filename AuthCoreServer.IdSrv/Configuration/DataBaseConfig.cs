using AuthCoreServer.IdSrv.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthCoreServer.IdSrv.Configuration
{
    public static class DataBaseConfig
    {
        public static void AddDataBaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddDbContext<AuthDBContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnectionString");
                 options.UseSqlServer(connectionString);
            });

            //return services

        }
    }
}
