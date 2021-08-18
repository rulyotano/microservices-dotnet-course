using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Infrastructure.Mail;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repositories;

namespace Ordering.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddDbContext<OrderContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OrderingConnectionString")));

            services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.Configure<EmailSettings>(c =>
            {
                var settingsSection = configuration.GetSection("EmailSettings");
                c.ApiKey = settingsSection["ApiKey"];
                c.FromAddress = settingsSection["FromAddress"];
                c.FromName = settingsSection["FromName"];
            });
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
