namespace OrderCreationService.Application.Contracts.Products.Operations;

public static class QueryProducts
{
    public readonly record struct Request(
        long[] Ids,
        string? NamePattern,
        decimal? MinPrice,
        decimal? MaxPrice,
        int Cursor,
        int PageSize);
}