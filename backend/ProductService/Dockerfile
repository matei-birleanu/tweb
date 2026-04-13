FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file if it exists
COPY ["*.sln", "./"]

# Copy project files
COPY ["ProductService/ProductService.csproj", "ProductService/"]
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["OrderService/OrderService.csproj", "OrderService/"]

# Restore dependencies
RUN dotnet restore "ProductService/ProductService.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/ProductService"
RUN dotnet build "ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]
