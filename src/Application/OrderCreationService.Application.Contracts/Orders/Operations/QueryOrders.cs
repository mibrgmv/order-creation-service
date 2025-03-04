using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Contracts.Orders.Operations;

public static class QueryOrders
{
    public readonly record struct Request(
        long[] Ids,
        OrderState? OrderState,
        string? CreatedBy,
        int Cursor,
        int PageSize);
}