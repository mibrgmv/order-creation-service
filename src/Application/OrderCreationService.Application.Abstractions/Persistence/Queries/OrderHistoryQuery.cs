using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Abstractions.Persistence.Queries;

public record OrderHistoryQuery(
    long[]? Ids,
    long[]? OrderIds,
    OrderHistoryItemKind? ItemKind,
    long Cursor,
    int PageSize);