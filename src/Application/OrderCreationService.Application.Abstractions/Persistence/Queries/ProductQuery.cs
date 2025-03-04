namespace OrderCreationService.Application.Abstractions.Persistence.Queries;

public record ProductQuery(
    long[]? Ids,
    string? NamePattern,
    decimal? MinPrice,
    decimal? MaxPrice,
    long? Cursor,
    int PageSize);