using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Abstractions.Queries;

public record OrderHistoryQuery(
    long[] Ids,
    long[]? OrderIds,
    OrderHistoryItemKind? ItemKind,
    int Cursor,
    int PageSize);