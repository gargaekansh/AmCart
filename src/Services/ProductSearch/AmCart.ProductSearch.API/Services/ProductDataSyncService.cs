

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Nest;
    using AmCart.ProductSearch.API.Entities;
    using AmCart.ProductSearch.API.Services.Interfaces;
    using AutoMapper;

    namespace AmCart.ProductSearch.API.Services
    {
        /// <summary>
        /// Service for syncing product data from MongoDB to Elasticsearch.
        /// </summary>
        public class ProductDataSyncService : IProductDataSyncService
        {
            private readonly IElasticClient _elasticClient;
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
            /// <param name="mongoDbName">Name of the MongoDB database.</param>
            /// <param name="mongoCollectionName">Name of the MongoDB collection containing products.</param>
            /// <param name="logger">Logger instance for logging.</param>
            /// <param name="mapper">AutoMapper instance for entity mapping.</param>
            public ProductDataSyncService(
                IElasticClient elasticClient,
                IMongoClient mongoClient,
                string mongoDbName,
                string mongoCollectionName,
                ILogger<ProductDataSyncService> logger,
                IMapper mapper)
            {
                _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

                // Access the MongoDB collection
                _productCollection = mongoClient.GetDatabase(mongoDbName)
                                                .GetCollection<Product>(mongoCollectionName);
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
                        await BulkIndexProductsAsync(batch);
                        return true; // Success
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Attempt {attempt}/{MaxRetryAttempts} failed for batch indexing.");
                        await Task.Delay(TimeSpan.FromSeconds(2 * attempt)); // Exponential backoff
                    }
                }
                return false; // Failed after all attempts
            }

            /// <summary>
            /// Performs bulk indexing of products into Elasticsearch.
            /// </summary>
            /// <param name="products">List of products to be indexed.</param>
            private async Task BulkIndexProductsAsync(List<AmCart.ProductSearch.API.Entities.ProductSearch> products)
            {
                var bulkRequest = new BulkRequest("products")
                {
                    Operations = products.Select(product => new BulkIndexOperation<AmCart.ProductSearch.API.Entities.ProductSearch>(product))
                                         .Cast<IBulkOperation>()
                                         .ToList()
                };

                var bulkResponse = await _elasticClient.BulkAsync(bulkRequest);

                if (!bulkResponse.IsValid)
                {
                    throw new Exception($"Elasticsearch bulk index error: {bulkResponse.DebugInformation}");
                }
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
