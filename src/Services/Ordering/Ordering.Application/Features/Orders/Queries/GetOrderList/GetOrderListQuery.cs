using MediatR;
using System.Collections.Generic;

namespace Ordering.Application.Features.Orders.Queries.GetOrderList
{
    public record GetOrderListQuery(string UserName): IRequest<List<OrderVm>>;
    
}
