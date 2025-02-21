using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Contracts.Products;

public interface IProductService
{
    Task<long[]> AddProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken);

    IAsyncEnumerable<Product> QueryProductsAsync(QueryProducts.Request request, CancellationToken cancellationToken);
}