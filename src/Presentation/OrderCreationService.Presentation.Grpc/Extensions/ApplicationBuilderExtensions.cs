using Microsoft.AspNetCore.Builder;
using OrderCreationService.Presentation.Grpc.Services;

namespace OrderCreationService.Presentation.Grpc.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePresentationGrpc(this IApplicationBuilder builder)
    {
        builder.UseEndpoints(routeBuilder =>
        {
            routeBuilder.MapGrpcService<ProductController>();
            routeBuilder.MapGrpcService<OrderController>();
            routeBuilder.MapGrpcReflectionService();
        });

        return builder;
    }
}