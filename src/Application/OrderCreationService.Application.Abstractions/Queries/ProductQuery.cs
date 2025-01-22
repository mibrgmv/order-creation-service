namespace OrderCreationService.Application.Abstractions.Queries;

public record ProductQuery(
    long[] Ids,
    string? NamePattern,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Cursor,
    int PageSize);