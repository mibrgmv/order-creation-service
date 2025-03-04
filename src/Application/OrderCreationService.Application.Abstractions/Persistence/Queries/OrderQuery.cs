using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Abstractions.Persistence.Queries;

public record OrderQuery(
    long[]? Ids,
    OrderState? OrderState,
    string? CreatedBy,
    long? Cursor,
    int PageSize);