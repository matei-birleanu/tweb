using AutoMapper;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ElasticsearchService _elasticsearchService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IMapper mapper,
        ElasticsearchService elasticsearchService,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        var products = await _repository.GetByCategoryAsync(category);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(SearchRequestDto request)
    {
        IEnumerable<Product> products;

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            // Try Elasticsearch first
            try
            {
                var esResults = await _elasticsearchService.SearchProductsAsync(request.Query);
                if (esResults.Any())
                {
                    products = esResults;
                }
                else
                {
                    // Fallback to database search
                    products = await _repository.SearchAsync(request.Query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Elasticsearch search failed, falling back to database");
                products = await _repository.SearchAsync(request.Query);
            }
        }
        else
        {
            products = await _repository.GetAllAsync();
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            products = products.Where(p => p.Category == request.Category);
        }

        if (request.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= request.MaxPrice.Value);
        }

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        var createdProduct = await _repository.CreateAsync(product);

        // Index in Elasticsearch
        try
        {
            await _elasticsearchService.IndexProductAsync(createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index product in Elasticsearch");
        }

        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto productDto)
    {
        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        _mapper.Map(productDto, existingProduct);
        var updatedProduct = await _repository.UpdateAsync(existingProduct);

        // Update in Elasticsearch
        try
        {
            await _elasticsearchService.UpdateProductAsync(updatedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update product in Elasticsearch");
        }

        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var result = await _repository.DeleteAsync(id);

        if (result)
        {
            // Delete from Elasticsearch
            try
            {
                await _elasticsearchService.DeleteProductAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete product from Elasticsearch");
            }
        }

        return result;
    }

    public async Task<bool> UpdateProductStockAsync(int id, int quantity)
    {
        return await _repository.UpdateStockAsync(id, quantity);
    }
}
