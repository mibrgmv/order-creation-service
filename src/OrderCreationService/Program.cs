using Microsoft.AspNetCore.Builder;
using OrderCreationService.Application.Services.Extensions;
using OrderCreationService.Infrastructure.Persistence.Extensions;
using OrderCreationService.Presentation.Grpc.Extensions;
using OrderCreationService.Presentation.Kafka.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence();
builder.Services.AddApplication();
builder.Services.AddPresentationKafka(builder.Configuration);
builder.Services.AddPresentationGrpc();

WebApplication app = builder.Build();

app.UseRouting();
app.UsePresentationGrpc();

await app.RunAsync();