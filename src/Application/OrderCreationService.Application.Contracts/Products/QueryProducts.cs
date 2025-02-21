namespace OrderCreationService.Application.Contracts.Products;

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