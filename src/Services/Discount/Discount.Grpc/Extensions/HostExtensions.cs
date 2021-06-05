using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.Grpc.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int retry = 0)
        {
            int retryForAvailability = retry;

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                MigrateDatabase(configuration, logger);
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(ex, "Exception migration postgres database");
                if (retryForAvailability > 50) return host;

                retryForAvailability++;
                System.Threading.Thread.Sleep(2000);
                return host.MigrateDatabase<TContext>(retryForAvailability);
            }

            return host;
        }

        private static void MigrateDatabase<TContext>(IConfiguration configuration, ILogger<TContext> logger)
        {
            logger.LogInformation("Migrating postgres database");

            using var connection = new NpgsqlConnection
                (configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            connection.Open();

            using var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = "DROP TABLE IF EXISTS Coupon"
            };
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                        ProductName VARCHAR(24) NOT NULL,
                                                        Description TEXT,
                                                        Amount INT)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
            command.ExecuteNonQuery();

            logger.LogInformation("Migrated postgres database.");
        }
    }

}
