using Grpc.Core;
using OrderCreationService.Application.Contracts.Products;
using OrderCreationService.Application.Models.Models;
using Products.CreationService.Contracts;

namespace OrderCreationService.Presentation.Grpc.Controllers;

public class ProductController : ProductService.ProductServiceBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
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
        var applicationRequest = new QueryProducts.Request(
            request.Ids.ToArray(),
            request.NamePattern,
            (decimal?)request.MinPrice,
            (decimal?)request.MaxPrice,
            request.Cursor,
            request.PageSize);

        await foreach (Product product in _productService.QueryProductsAsync(applicationRequest, context.CancellationToken))
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