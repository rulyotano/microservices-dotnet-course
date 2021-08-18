using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext context, ILogger<OrderContextSeed> logger)
        {
            if (context.Orders.Any())
                return;
            await context.Orders.AddRangeAsync(GetPreconfiguredOrders());
            await context.SaveChangesAsync();
            logger.LogInformation("Seed database associated with context {DbContextName}", nameof(OrderContext));
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order()
                {
                    UserName = "swn",
                    FirstName = "Mehmet",
                    LastName = "Ozkaya",
                    EmailAddress = "ezozkme@gmail.com",
                    AddressLine = "Bahcelievler",
                    Country = "Turkey",
                    TotalPrice = 350
                }
            };
        }
    }
}
