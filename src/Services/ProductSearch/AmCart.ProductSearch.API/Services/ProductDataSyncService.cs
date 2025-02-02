using MongoDB.Driver;
using AmCart.ProductSearch.API.Entities;
using AmCart.ProductSearch.API.Services.Interfaces;
using AutoMapper;
using AmCart.ProductSearch.API.Configuration;
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;

namespace AmCart.ProductSearch.API.Services
{
    /// <summary>
    /// Service for syncing product data from MongoDB to Elasticsearch.
    /// </summary>
    public class ProductDataSyncService : IProductDataSyncService
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly ILogger<ProductDataSyncService> _logger;
        private readonly IMapper _mapper;

        private const int BatchSize = 100; // Process 100 products per batch
        private const int MaxRetryAttempts = 3; // Retry failed batches up to 3 times

        /// <summary>
        /// Initializes a new instance of <see cref="ProductDataSyncService"/>.
        /// </summary>
        /// <param name="elasticClient">Elasticsearch client for indexing products.</param>
        /// <param name="mongoClient">MongoDB client for retrieving product data.</param>
        /// <param name="mongoSettings">MongoDB settings retrieved from configuration.</param>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="mapper">AutoMapper instance for entity mapping.</param>
        public ProductDataSyncService(
            ElasticsearchClient elasticClient,
            IMongoClient mongoClient,
            IOptions<MongoDbSettings> mongoSettings,
            ILogger<ProductDataSyncService> logger,
            IMapper mapper)
        {
            if (mongoSettings == null || mongoSettings.Value == null)
            {
                throw new ArgumentNullException(nameof(mongoSettings), "MongoDB settings cannot be null.");
            }

            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            // Access the MongoDB collection using settings
            _productCollection = mongoClient
                .GetDatabase(mongoSettings.Value.DatabaseName)
                .GetCollection<Product>(mongoSettings.Value.CollectionName);
        }


            /// <summary>
            /// Synchronizes product data from MongoDB to Elasticsearch in batches.
            /// </summary>
            public async Task SyncProductDataAsync()
            {
                try
                {
                    _logger.LogInformation("🔄 Starting product data sync from MongoDB to Elasticsearch...");

                    // Fetch all products from MongoDB
                    var products = await FetchProductsFromMongoDbAsync();

                    if (!products.Any())
                    {
                        _logger.LogWarning("⚠️ No products found in MongoDB.");
                        return;
                    }

                    // Convert MongoDB Product to Elasticsearch ProductSearch entity
                    var productSearchList = _mapper.Map<List<AmCart.ProductSearch.API.Entities.ProductSearch>>(products);

                    // Process indexing in batches
                    await IndexProductsInBatchesAsync(productSearchList);

                    _logger.LogInformation("✅ Product data sync completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error occurred during product data sync.");
                }
            }

            /// <summary>
            /// Fetches all products from MongoDB.
            /// </summary>
            /// <returns>List of MongoDB products.</returns>
            private async Task<List<Product>> FetchProductsFromMongoDbAsync()
            {
                return await _productCollection.Find(FilterDefinition<Product>.Empty).ToListAsync();
            }

            /// <summary>
            /// Indexes products into Elasticsearch in batches with retry logic.
            /// </summary>
            /// <param name="products">List of products to be indexed.</param>
            private async Task IndexProductsInBatchesAsync(List<AmCart.ProductSearch.API.Entities.ProductSearch> products)
            {
                int totalProducts = products.Count;
                int batchCount = (int)Math.Ceiling(totalProducts / (double)BatchSize);

                _logger.LogInformation($"📦 Processing {totalProducts} products in {batchCount} batches...");

                for (int i = 0; i < batchCount; i++)
                {
                    var batch = products.Skip(i * BatchSize).Take(BatchSize).ToList();

                    bool success = await TryBulkIndexWithRetries(batch);

                    if (success)
                    {
                        _logger.LogInformation($"✅ Successfully indexed batch {i + 1}/{batchCount} ({batch.Count} products).");
                    }
                    else
                    {
                        _logger.LogError($"❌ Failed to index batch {i + 1}/{batchCount} after multiple retries.");
                    }
                }
            }

        /// <summary>
        /// Attempts to bulk index a batch of products with retry logic.
        /// </summary>
        /// <param name="batch">List of products to be indexed.</param>
        /// <returns>True if indexing succeeded, false otherwise.</returns>
        private async Task<bool> TryBulkIndexWithRetries(List<AmCart.ProductSearch.API.Entities.ProductSearch> batch)
        {
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    var success = await BulkIndexProductsAsync(batch);
                    if (success) return true; // Success
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"⚠️ Attempt {attempt}/{MaxRetryAttempts} failed for batch indexing.");

                    // Add exponential backoff to avoid overwhelming Elasticsearch
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
                }
            }
            return false; // Failed after all attempts
        }


        /// <summary>
        /// Performs bulk indexing of products into Elasticsearch.
        /// </summary>
        /// <param name="products">List of products to be indexed.</param>
        private async Task<bool> BulkIndexProductsAsync(List<AmCart.ProductSearch.API.Entities.ProductSearch> products)
        {
            var existingProductIds = new List<string>(); // Store product IDs that already exist
            var bulkRequest = new BulkRequest("products")
            {
                Operations = new List<IBulkOperation>()
            };

            foreach (var product in products)
            {
                var productId = product.Id.ToString(); // Assuming the product ID is a unique identifier

                // Check if the product already exists in Elasticsearch using GetAsync
                var getResponse = await _elasticClient.GetAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(productId, g => g.Index("products"));

                if (getResponse.Found)
                {
                    // Product already exists in Elasticsearch, skip it or log it
                    existingProductIds.Add(productId);
                    _logger.LogInformation($"Product {productId} already exists in Elasticsearch. Skipping index.");
                    continue; // Skip indexing this product
                }

                // Add the product to the bulk request for indexing
                bulkRequest.Operations.Add(new BulkIndexOperation<AmCart.ProductSearch.API.Entities.ProductSearch>(product));
            }

            if (bulkRequest.Operations.Count == 0)
            {
                _logger.LogInformation("No new products to index.");
                return true; // No products to index
            }

            // Perform the bulk indexing
            var bulkResponse = await _elasticClient.BulkAsync(bulkRequest);

            if (!bulkResponse.IsValidResponse)
            {
                _logger.LogError($"❌ Elasticsearch bulk index error: {bulkResponse.DebugInformation}");
                return false;
            }

            if (bulkResponse.Errors)
            {
                foreach (var item in bulkResponse.ItemsWithErrors)
                {
                    _logger.LogWarning($"⚠️ Document ID {item.Id} failed: {item.Error.Reason}");
                }

                return false;
            }

            // Log products that were successfully indexed and the ones skipped
            _logger.LogInformation($"✅ Successfully indexed {bulkResponse.Items.Count} new products.");
            if (existingProductIds.Any())
            {
                _logger.LogInformation($"⛔ Skipped {existingProductIds.Count} existing products.");
            }

            return true;
        }


    }
}




    //public class ProductDataSyncService: IProductDataSyncService
    //{
    //    private readonly IElasticClient _elasticClient;
    //    private readonly IMongoCollection<Product> _productCollection;

    //    public ProductDataSyncService(IElasticClient elasticClient, IMongoClient mongoClient, string mongoDbName, string mongoCollectionName)
    //    {
    //        _elasticClient = elasticClient;

    //        // Access the MongoDB collection for products
    //        _productCollection = mongoClient.GetDatabase(mongoDbName).GetCollection<Product>(mongoCollectionName);
    //    }

    //    // Sync MongoDB product data to Elasticsearch
    //    public async Task SyncProductDataAsync()
    //    {
    //        // Fetch product data from MongoDB
    //        var products = await FetchProductsFromMongoDb();

    //        // Insert the products into Elasticsearch
    //        var success = await InsertProductsIntoElasticsearch(products);

    //        if (success)
    //        {
    //            Console.WriteLine("✅ Product data synced to Elasticsearch successfully.");
    //        }
    //        else
    //        {
    //            Console.WriteLine("❌ Error syncing product data.");
    //        }
    //    }

    //    private async Task<List<Product>> FetchProductsFromMongoDb()
    //    {
    //        // Fetch all products from the MongoDB collection
    //        return await _productCollection.Find(FilterDefinition<Product>.Empty).ToListAsync();
    //    }

    //    private async Task<bool> InsertProductsIntoElasticsearch(List<Product> products)
    //    {
    //        // Prepare bulk request to insert products into Elasticsearch
    //        var bulkRequest = new BulkRequest("products")
    //        {
    //            Operations = products.Select(product => new BulkIndexOperation<Product>(product)).Cast<IBulkOperation>().ToList()
    //        };

    //        var bulkResponse = await _elasticClient.BulkAsync(bulkRequest);

    //        return bulkResponse.IsValid;
    //    }
    //}
//}
