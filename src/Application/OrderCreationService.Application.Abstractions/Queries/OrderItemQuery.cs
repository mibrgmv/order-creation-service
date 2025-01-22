namespace OrderCreationService.Application.Abstractions.Queries;

public record OrderItemQuery(
    long[] Ids,
    long[]? OrderIds,
    long[]? ProductIds,
    bool? Deleted,
    int Cursor,
    int PageSize);