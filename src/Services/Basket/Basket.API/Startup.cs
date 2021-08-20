using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.API.Repositories;
using Basket.API.GrpcServices;
using Discount.Grpc.Protos;
using MassTransit;

namespace Basket.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDiscountGrpcService, DiscountGrpcService>();
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));

            ConfigureGrpc(services);
            ConfigureRedis(services);
            ConfigureRabbitMqMassTransit(services);
            ConfigureSwagger(services);
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Basket.API", Version = "v1"}); });
        }

        private void ConfigureRabbitMqMassTransit(IServiceCollection services)
        {
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((context, rabbitMqConfig) =>
                {
                    rabbitMqConfig.Host(Configuration["EventBusSettings:HostAddress"]);
                });
            });
            services.AddMassTransitHostedService();
        }

        private void ConfigureGrpc(IServiceCollection services)
        {
            services.AddGrpcClient<DiscountProService.DiscountProServiceClient>(options =>
            {
                options.Address = new Uri(Configuration["GrpcSettings:DiscountUrl"]);
            });
        }

        private void ConfigureRedis(IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var connectionString = Configuration.GetValue<string>("CacheSettings:ConnectionString");
                options.Configuration = connectionString;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
