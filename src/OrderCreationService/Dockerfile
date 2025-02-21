FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OrderCreationService/OrderCreationService.csproj", "src/OrderCreationService/"]
COPY ["src/Presentation/OrderCreationService.Presentation.Grpc/OrderCreationService.Presentation.Grpc.csproj", "src/Presentation/OrderCreationService.Presentation.Grpc/"]
COPY ["src/Application/OrderCreationService.Application.Services/OrderCreationService.Application.Services.csproj", "src/Application/OrderCreationService.Application.Services/"]
COPY ["src/Application/OrderCreationService.Application.Abstractions/OrderCreationService.Application.Abstractions.csproj", "src/Application/OrderCreationService.Application.Abstractions/"]
COPY ["src/Application/OrderCreationService.Application.Models/OrderCreationService.Application.Models.csproj", "src/Application/OrderCreationService.Application.Models/"]
COPY ["src/Application/OrderCreationService.Application.Contracts/OrderCreationService.Application.Contracts.csproj", "src/Application/OrderCreationService.Application.Contracts/"]
COPY ["src/Presentation/OrderCreationService.Presentation.Kafka/OrderCreationService.Presentation.Kafka.csproj", "src/Presentation/OrderCreationService.Presentation.Kafka/"]
COPY ["src/Infrastructure/OrderCreationService.Infrastructure.Kafka/OrderCreationService.Infrastructure.Kafka.csproj", "src/Infrastructure/OrderCreationService.Infrastructure.Kafka/"]
COPY ["src/Infrastructure/OrderCreationService.Infrastructure.Persistence/OrderCreationService.Infrastructure.Persistence.csproj", "src/Infrastructure/OrderCreationService.Infrastructure.Persistence/"]
RUN dotnet restore "src/OrderCreationService/OrderCreationService.csproj"
COPY . .
WORKDIR "/src/src/OrderCreationService"
RUN dotnet build "OrderCreationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrderCreationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderCreationService.dll"]
