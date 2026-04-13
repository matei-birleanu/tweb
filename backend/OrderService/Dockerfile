FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file if it exists
COPY ["*.sln", "./"]

# Copy project files
COPY ["OrderService/OrderService.csproj", "OrderService/"]
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["ProductService/ProductService.csproj", "ProductService/"]

# Restore dependencies
RUN dotnet restore "OrderService/OrderService.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/OrderService"
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
