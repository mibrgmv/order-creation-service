using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Abstractions.Queries;

public record OrderHistoryQuery(
    long[] Ids,
    long[]? OrderIds,
    OrderHistoryItemKind? Kind,
    int Cursor,
    int PageSize);