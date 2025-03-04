using OrderCreationService.Application.Abstractions.Persistence.Queries;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Persistence.Repositories;

public interface IProductRepository
{
    Task<long[]> AddProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken);

    IAsyncEnumerable<Product> QueryProductsAsync(ProductQuery query, CancellationToken cancellationToken);
}