FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Pedidos.sln ./
COPY src/Pedidos.Domain/Pedidos.Domain.csproj src/Pedidos.Domain/
COPY src/Pedidos.Application/Pedidos.Application.csproj src/Pedidos.Application/
COPY src/Pedidos.Infrastructure/Pedidos.Infrastructure.csproj src/Pedidos.Infrastructure/
COPY src/Pedidos.Api/Pedidos.Api.csproj src/Pedidos.Api/
COPY tests/Pedidos.Domain.Tests/Pedidos.Domain.Tests.csproj tests/Pedidos.Domain.Tests/
COPY tests/Pedidos.Application.Tests/Pedidos.Application.Tests.csproj tests/Pedidos.Application.Tests/

RUN dotnet restore Pedidos.sln

COPY src/ src/
COPY tests/ tests/

RUN dotnet publish src/Pedidos.Api/Pedidos.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Pedidos.Api.dll"]
