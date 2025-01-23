using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Abstractions.Repositories;
using OrderCreationService.Application.Abstractions.Services;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Services.Services;

internal sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<long[]> AddProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken)
    {
        return await _productRepository.AddProductsAsync(products, cancellationToken);
    }

    public async Task<Product> GetProductAsync(long productId, CancellationToken cancellationToken)
    {
        return await _productRepository.GetProductAsync(productId, cancellationToken);
    }

    public IAsyncEnumerable<Product> QueryProductsAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        return _productRepository.QueryProductsAsync(query, cancellationToken);
    }
}