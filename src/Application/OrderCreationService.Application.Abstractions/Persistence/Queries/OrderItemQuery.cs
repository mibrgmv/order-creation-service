namespace OrderCreationService.Application.Abstractions.Persistence.Queries;

public record OrderItemQuery(
    long[]? Ids,
    long[]? OrderIds,
    long[]? ProductIds,
    bool? Deleted,
    long? Cursor,
    int PageSize);