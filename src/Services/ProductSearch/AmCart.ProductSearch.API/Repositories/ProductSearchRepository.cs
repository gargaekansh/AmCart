using AmCart.ProductSearch.API.Repositories.Interfaces;
using Elastic.Clients.Elasticsearch; // Updated namespace for new Elastic client

namespace AmCart.ProductSearch.API.Repositories
{
    public class ProductSearchRepository : IProductSearchRepository
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ILogger<ProductSearchRepository> _logger;

        public ProductSearchRepository(ElasticsearchClient elasticClient, ILogger<ProductSearchRepository> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        /// <summary>
        /// Insert or update a product in Elasticsearch.
        /// </summary>
        /// <param name="product">Product entity</param>
        /// <returns></returns>
        public async Task InsertOrUpdateProductAsync(Entities.ProductSearch product)
        {
            var response = await _elasticClient.IndexAsync(product, idx => idx
                .Index("products")
                .Id(product.Id)
                .Refresh(Refresh.WaitFor) // Ensure the change is visible immediately
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to insert/update product {ProductId}. Error: {Error}", product.Id, response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Product {ProductId} indexed successfully", product.Id);
            }
        }

        /// <summary>
        /// Bulk inserts multiple products into Elasticsearch.
        /// </summary>
        /// <param name="products">List of products to index</param>
        /// <returns>Boolean indicating success or failure</returns>
        public async Task<bool> BulkInsertProductsAsync(IEnumerable<AmCart.ProductSearch.API.Entities.ProductSearch> products)
        {
            if (products == null || !products.Any())
            {
                _logger.LogWarning("No products provided for bulk insertion.");
                return false;
            }

            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index("products")
                .IndexMany(products)
            );

            if (!bulkResponse.IsValidResponse)
            {
                _logger.LogError("Bulk insert failed. Errors: {Errors}", bulkResponse.DebugInformation);
                return false;
            }

            _logger.LogInformation("Successfully inserted {Count} products into Elasticsearch.", products.Count());
            return true;
        }

        /// <summary>
        /// Delete a product from Elasticsearch.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns></returns>
        public async Task DeleteProductAsync(string productId)
        {
            var response = await _elasticClient.DeleteAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(productId, d => d
                .Index("products")
                .Refresh(Refresh.WaitFor) // Ensure deletion is immediately visible
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to delete product {ProductId}. Error: {Error}", productId, response.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Product {ProductId} deleted successfully", productId);
            }
        }

        /// <summary>
        /// Full-text, fuzzy search, and autocomplete for products, prioritizing matches in Name > Category > Description.
        /// </summary>
        /// <param name="query">Search query</param>
        /// <returns>Search results</returns>
        public async Task<Elastic.Clients.Elasticsearch.ISearchResponse<AmCart.ProductSearch.API.Entities.ProductSearch>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Return all products if query is empty
                return await _elasticClient.SearchAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(s => s
                    .Index("products")
                    .Query(q => q.MatchAll())
                );
            }

            var searchResponse = await _elasticClient.SearchAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(s => s
                .Index("products")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            // Full-text search with boosting applied separately
                            sh => sh.MultiMatch(m => m
                                .Fields(f => f
                                    .Field(p => p.Name)
                                    .Field(p => p.Category)
                                    .Field(p => p.Description)
                                )
                                .Query(query)
                                .Type(Elastic.Clients.Elasticsearch.TextQueryType.BestFields)
                                .Fuzziness(Elastic.Clients.Elasticsearch.Fuzziness.Auto)
                                .PrefixLength(2)
                                .Boost(2.0) // Boosting the entire full-text search
                            ),
                            // Autocomplete for Name (highest priority)
                            sh => sh.MatchPhrasePrefix(mpp => mpp
                                .Field(p => p.Name)
                                .Query(query)
                                .Boost(3.0) // Highest priority
                            ),
                            // Autocomplete for Category (medium priority)
                            sh => sh.MatchPhrasePrefix(mpp => mpp
                                .Field(p => p.Category)
                                .Query(query)
                                .Boost(2.0) // Medium priority
                            )
                        )
                    )
                )
                .Sort(s => s
                    .Descending(Elastic.Clients.Elasticsearch.SortSpecialField.Score) // Prioritize high-relevance results
                )
                .Size(50) // Limits results to 50 for performance
            );

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Search query failed. Errors: {Errors}", searchResponse.DebugInformation);
            }

            return searchResponse;
        }
    }
}
