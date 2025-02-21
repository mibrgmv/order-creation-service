using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Abstractions.Repositories;
using OrderCreationService.Application.Contracts.Products;
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

    public IAsyncEnumerable<Product> QueryProductsAsync(
        QueryProducts.Request request,
        CancellationToken cancellationToken)
    {
        var query = new ProductQuery(
            Ids: request.Ids,
            NamePattern: request.NamePattern,
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice,
            Cursor: request.Cursor,
            PageSize: request.PageSize);

        return _productRepository.QueryProductsAsync(query, cancellationToken);
    }
}