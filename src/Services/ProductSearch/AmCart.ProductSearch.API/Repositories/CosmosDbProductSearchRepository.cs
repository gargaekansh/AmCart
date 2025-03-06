using AmCart.ProductSearch.API.Configuration;
using AmCart.ProductSearch.API.DTO;
using AmCart.ProductSearch.API.Entities;
using AmCart.ProductSearch.API.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;

namespace AmCart.ProductSearch.API.Repositories
{
    /// <summary>
    /// Repository for searching products in CosmosDB (MongoDB API).
    /// </summary>
    public class CosmosDbProductSearchRepository : IProductSearchRepository
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly ILogger<CosmosDbProductSearchRepository> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbProductSearchRepository"/> class.
        /// </summary>
        /// <param name="mongoClient">MongoDB client instance.</param>
        /// <param name="settings">MongoDB settings from configuration.</param>
        /// <param name="logger">Logger instance.</param>
        public CosmosDbProductSearchRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbSettings> settings,
            ILogger<CosmosDbProductSearchRepository> logger
            , IMapper mapper)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _productCollection = database.GetCollection<Product>(settings.Value.CollectionName);
            _logger = logger;
            _mapper = mapper;
        }

        ///// <summary>
        ///// Searches for products by category and/or keyword using regex-based search.
        ///// </summary>
        //public async Task<IEnumerable<Product>> SearchProductsAsync(string query, string category = null)
        //{
        //    var filters = new List<FilterDefinition<Product>>();
        //    var builder = Builders<Product>.Filter;

        //    if (!string.IsNullOrEmpty(query))
        //    {
        //        var regexFilter = builder.Or(
        //            builder.Regex(p => p.Name, new BsonRegularExpression(query, "i")), // Case-insensitive regex
        //            builder.Regex(p => p.Description, new BsonRegularExpression(query, "i"))
        //        );
        //        filters.Add(regexFilter);
        //    }

        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        filters.Add(builder.Eq(p => p.Category, category));
        //    }

        //    var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;
        //    return await _productCollection.Find(filter).Limit(50).ToListAsync();
        //}

        /// <summary>
        /// Performs a smart search in CosmosDB, detecting whether the query is a category or a general search term.
        /// </summary>
        /// <param name="query">The search term.</param>
        /// <returns>A standardized search response containing matching products.</returns>
        public async Task<ProductSearchResponse<Entities.ProductSearch>> SearchAsync(string query)
        {
            var builder = Builders<Product>.Filter;

            try
            {
                // ✅ Check if the query is an exact match for a category
                bool isCategory = await _productCollection
                    .Find(builder.Eq(p => p.Category, query))
                    .AnyAsync();

                List<Product> results;

                if (isCategory)
                {
                    _logger.LogInformation("🔍 Detected '{Query}' as a category.", query);
                    results = await _productCollection
                        .Find(builder.Eq(p => p.Category, query))
                        .Limit(50)
                        .ToListAsync();
                }
                else
                {
                    _logger.LogInformation("🔍 Performing regex-based search for '{Query}'.", query);
                    var regexFilter = builder.Or(
                        builder.Regex(p => p.Name, new BsonRegularExpression(query, "i")),
                        builder.Regex(p => p.Description, new BsonRegularExpression(query, "i"))
                    );

                    results = await _productCollection.Find(regexFilter).Limit(50).ToListAsync();
                }

                // ✅ Use AutoMapper to map `Product` to `ProductSearch`
                var mappedResults = _mapper.Map<List<Entities.ProductSearch>>(results);

                // ✅ Return the standardized response
                return new ProductSearchResponse<Entities.ProductSearch>
                {
                    Results = mappedResults,
                    TotalCount = results.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error occurred during SmartSearchAsync for query: {Query}", query);
                throw;
            }
        }



        //////////////////////////   Not Compaitable to v4.0 ///////////////////////////////////////

        ///// <summary>
        ///// Searches for products by category and/or keyword using full-text search.
        ///// </summary>
        ///// <param name="query">Search keyword for product name or description.</param>
        ///// <param name="category">Optional category filter.</param>
        ///// <returns>A list of products matching the search criteria.</returns>
        //public async Task<IEnumerable<Product>> SearchProductsAsync(string query, string category = null)
        //{
        //    var searchStage = new BsonDocument("$search", new BsonDocument
        //    {
        //        { "index", "default" }, // The text index name
        //        { "compound", new BsonDocument
        //            {
        //                { "should", new BsonArray
        //                    {
        //                        new BsonDocument("autocomplete", new BsonDocument
        //                        {
        //                            { "query", query },
        //                            { "path", "name" },
        //                            { "fuzzy", new BsonDocument { { "maxEdits", 1 } } },
        //                            { "score", new BsonDocument { { "boost", new BsonDocument { { "value", 3.0 } } } } }
        //                        }),
        //                        new BsonDocument("text", new BsonDocument
        //                        {
        //                            { "query", query },
        //                            { "path", "description" },
        //                            { "fuzzy", new BsonDocument { { "maxEdits", 2 } } },
        //                            { "score", new BsonDocument { { "boost", new BsonDocument { { "value", 2.0 } } } } }
        //                        })
        //                    }
        //                }
        //            }
        //        }
        //    });

        //    var pipeline = new List<BsonDocument> { searchStage };

        //    // Add category filter if provided
        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        pipeline.Add(new BsonDocument("$match", new BsonDocument("category", category)));
        //    }

        //    // Limit results to 50 items
        //    pipeline.Add(new BsonDocument("$limit", 50));

        //    return await _productCollection.Aggregate<Product>(pipeline).ToListAsync();
        //}

        ///// <summary>
        ///// Searches products based on whether the input is a category or a general search term.
        ///// If the input matches an existing category, it filters by category.
        ///// Otherwise, it performs a full-text search with autocomplete on `name` and `description`.
        ///// </summary>
        ///// <param name="searchTerm">User input that could be a category or a keyword.</param>
        ///// <returns>List of matching products.</returns>
        //public async Task<IEnumerable<Product>> SmartSearchAsync(string searchTerm)
        //{
        //    var filters = new List<FilterDefinition<Product>>();

        //    // Check if the search term is an existing category
        //    bool isCategory = await _productCollection
        //        .Find(Builders<Product>.Filter.Eq(p => p.Category, searchTerm))
        //        .AnyAsync();

        //    if (isCategory)
        //    {
        //        _logger.LogInformation($"🔍 Detected '{searchTerm}' as a category.");
        //        filters.Add(Builders<Product>.Filter.Eq(p => p.Category, searchTerm));
        //    }
        //    else
        //    {
        //        _logger.LogInformation($"🔍 Performing full-text search for '{searchTerm}'.");

        //        // Full-text search with autocomplete
        //        var textSearchPipeline = new BsonDocument[]
        //        {
        //        new BsonDocument("$search", new BsonDocument
        //        {
        //            { "index", "default" }, // Must match your search index name
        //            { "compound", new BsonDocument
        //                {
        //                    { "should", new BsonArray
        //                        {
        //                            new BsonDocument("autocomplete", new BsonDocument
        //                            {
        //                                { "query", searchTerm },
        //                                { "path", "name" },
        //                                { "fuzzy", new BsonDocument { { "maxEdits", 1 } } },
        //                                { "score", new BsonDocument { { "boost", new BsonDocument { { "value", 3.0 } } } } }
        //                            }),
        //                            new BsonDocument("text", new BsonDocument
        //                            {
        //                                { "query", searchTerm },
        //                                { "path", "description" },
        //                                { "fuzzy", new BsonDocument { { "maxEdits", 2 } } },
        //                                { "score", new BsonDocument { { "boost", new BsonDocument { { "value", 2.0 } } } } }
        //                            })
        //                        }
        //                    }
        //                }
        //            }
        //        }),
        //        new BsonDocument("$limit", 50)
        //        };

        //        return await _productCollection.Aggregate<Product>(textSearchPipeline).ToListAsync();
        //    }

        //    var combinedFilter = Builders<Product>.Filter.And(filters);
        //    return await _productCollection.Find(combinedFilter).Limit(50).ToListAsync();
        //}
    }
}
