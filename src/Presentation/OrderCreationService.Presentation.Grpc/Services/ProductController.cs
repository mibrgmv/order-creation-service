using Grpc.Core;
using OrderCreationService.Application.Abstractions.Services;
using OrderCreationService.Application.Models.Models;
using Products.CreationService.Contracts;

namespace OrderCreationService.Presentation.Grpc.Services;

public class ProductController : ProductService.ProductServiceBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        Product product = await _productService.GetProductAsync(request.Id, context.CancellationToken);
        var dto = new ProductDto { Name = product.Name, Price = (double)product.Price };
        return new GetProductResponse { Product = dto };
    }

    public override async Task<AddProductsResponse> AddProducts(AddProductsRequest request, ServerCallContext context)
    {
        List<Product> products = [];

        products.AddRange(request.Products
            .Select(dto =>
                new Product(
                    Id: default,
                    Name: dto.Name,
                    Price: (decimal)dto.Price)));

        long[] ids = await _productService.AddProductsAsync(products.ToArray(), context.CancellationToken);
        return new AddProductsResponse { ProductsIds = { ids } };
    }

    public override async Task QueryProducts(ProductQuery request, IServerStreamWriter<ProductDto> responseStream, ServerCallContext context)
    {
        var query = new Application.Abstractions.Queries.ProductQuery(
            request.Ids.ToArray(),
            request.NamePattern,
            (decimal?)request.MinPrice,
            (decimal?)request.MaxPrice,
            request.Cursor,
            request.PageSize);

        await foreach (Product product in _productService.QueryProductsAsync(query, context.CancellationToken))
        {
            await responseStream.WriteAsync(
                new ProductDto
                {
                    Name = product.Name,
                    Price = (double)product.Price,
                },
                context.CancellationToken);
        }
    }
}