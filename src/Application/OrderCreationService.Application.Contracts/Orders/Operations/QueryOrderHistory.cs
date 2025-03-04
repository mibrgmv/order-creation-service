using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Contracts.Orders.Operations;

public static class QueryOrderHistory
{
    public readonly record struct Request(
        long[] Ids,
        long[]? OrderIds,
        OrderHistoryItemKind? ItemKind,
        int Cursor,
        int PageSize);
}