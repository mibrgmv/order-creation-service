namespace OrderCreationService.Application.Contracts.Orders.Requests;

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