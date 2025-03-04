namespace OrderCreationService.Application.Contracts.Orders.Operations;

public static class QueryOrderItems
{
    public readonly record struct Request(
        long[] Ids,
        long[]? OrderIds,
        long[]? ProductIds,
        bool? Deleted,
        int Cursor,
        int PageSize);
}