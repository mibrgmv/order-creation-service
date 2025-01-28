using Microsoft.Extensions.DependencyInjection;
using OrderCreationService.Presentation.Grpc.Interceptors;

namespace OrderCreationService.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationGrpc(this IServiceCollection collection)
    {
        collection.AddGrpc(grpc => grpc.Interceptors.Add<LoggerInterceptor>());
        collection.AddGrpcReflection();

        return collection;
    }
}