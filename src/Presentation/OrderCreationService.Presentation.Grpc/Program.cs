using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OrderCreationService.Application.Services.Extensions;
using OrderCreationService.Infrastructure.Persistence;
using OrderCreationService.Infrastructure.Persistence.Extensions;
using OrderCreationService.Presentation.Grpc.Interceptors;
using OrderCreationService.Presentation.Grpc.Services;
using OrderCreationService.Presentation.Kafka.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<PersistenceOptions>().BindConfiguration(nameof(PersistenceOptions));
builder.Services.AddPersistence();

builder.Services.AddPresentationKafka(builder.Configuration);

builder.Services.AddServices();
builder.Services.AddGrpc(grpc => grpc.Interceptors.Add<LoggerInterceptor>());
builder.Services.AddGrpcReflection();

WebApplication app = builder.Build();

app.MapGrpcService<ProductController>();
app.MapGrpcService<OrderController>();
app.MapGrpcReflectionService();

app.Run();