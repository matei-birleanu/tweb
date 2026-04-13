using Nest;
using ProductService.Models;

namespace ProductService.Services;

public class ElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly string _indexName;

    public ElasticsearchService(IConfiguration configuration, ILogger<ElasticsearchService> logger)
    {
        _logger = logger;
        _indexName = configuration["Elasticsearch:IndexName"] ?? "products";

        var url = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
        var settings = new ConnectionSettings(new Uri(url))
            .DefaultIndex(_indexName)
            .DefaultMappingFor<Product>(m => m.IndexName(_indexName));

        _client = new ElasticClient(settings);

        InitializeIndexAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeIndexAsync()
    {
        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(_indexName);
            if (!existsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync(_indexName, c => c
                    .Map<Product>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t.Name(n => n.Name).Analyzer("standard"))
                            .Text(t => t.Name(n => n.Description).Analyzer("standard"))
                            .Keyword(k => k.Name(n => n.Category))
                        )
                    )
                );

                if (createIndexResponse.IsValid)
                {
                    _logger.LogInformation("Elasticsearch index created: {IndexName}", _indexName);
                }
                else
                {
                    _logger.LogWarning("Failed to create Elasticsearch index: {Error}", createIndexResponse.DebugInformation);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Elasticsearch index");
        }
    }

    public async Task IndexProductAsync(Product product)
    {
        try
        {
            var response = await _client.IndexDocumentAsync(product);
            if (!response.IsValid)
            {
                _logger.LogWarning("Failed to index product: {Error}", response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing product in Elasticsearch");
            throw;
        }
    }

    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            var response = await _client.UpdateAsync<Product>(product.Id, u => u
                .Doc(product)
                .DocAsUpsert(true)
            );

            if (!response.IsValid)
            {
                _logger.LogWarning("Failed to update product in Elasticsearch: {Error}", response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product in Elasticsearch");
            throw;
        }
    }

    public async Task DeleteProductAsync(int productId)
    {
        try
        {
            var response = await _client.DeleteAsync<Product>(productId);
            if (!response.IsValid)
            {
                _logger.LogWarning("Failed to delete product from Elasticsearch: {Error}", response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product from Elasticsearch");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        try
        {
            var searchResponse = await _client.SearchAsync<Product>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields(f => f
                            .Field(p => p.Name, boost: 2.0)
                            .Field(p => p.Description)
                            .Field(p => p.Category)
                        )
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
                .Size(100)
            );

            if (searchResponse.IsValid)
            {
                return searchResponse.Documents;
            }

            _logger.LogWarning("Elasticsearch search failed: {Error}", searchResponse.DebugInformation);
            return Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products in Elasticsearch");
            throw;
        }
    }
}
