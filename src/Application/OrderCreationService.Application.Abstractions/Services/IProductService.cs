using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Services;

public interface IProductService
{
    Task<long[]> AddProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken);

    Task<Product> GetProductAsync(long productId, CancellationToken cancellationToken);

    IAsyncEnumerable<Product> QueryProductsAsync(ProductQuery query, CancellationToken cancellationToken);
}