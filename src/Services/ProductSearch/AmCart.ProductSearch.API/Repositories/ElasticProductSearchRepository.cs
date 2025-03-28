﻿using AmCart.ProductSearch.API.DTO;
using AmCart.ProductSearch.API.Repositories.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;


namespace AmCart.ProductSearch.API.Repositories
{
    public class ElasticProductSearchRepository : IProductSearchRepository
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ILogger<ElasticProductSearchRepository> _logger;

        public ElasticProductSearchRepository(ElasticsearchClient elasticClient, ILogger<ElasticProductSearchRepository> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        ///// <summary>
        ///// Inserts or updates a product in Elasticsearch.
        ///// </summary>
        ///// <param name="product">Product entity to insert or update.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task InsertOrUpdateProductAsync(Entities.ProductSearch product)
        //{
        //    var response = await _elasticClient.IndexAsync(product, idx => idx
        //        .Index("products")  // Index where the product is stored
        //        .Id(product.Id)  // Use the product ID as the document ID
        //        .Refresh(Refresh.WaitFor)  // Ensure the change is visible immediately
        //    );

        //    if (!response.IsValidResponse)
        //    {
        //        _logger.LogError("Failed to insert/update product {ProductId}. Error: {Error}", product.Id, response.DebugInformation);
        //    }
        //    else
        //    {
        //        _logger.LogInformation("Product {ProductId} indexed successfully", product.Id);
        //    }
        //}

        ///// <summary>
        ///// Performs a bulk insert of multiple products into Elasticsearch.
        ///// </summary>
        ///// <param name="products">List of products to index.</param>
        ///// <returns>A boolean indicating success or failure of the bulk insert.</returns>
        //public async Task<bool> BulkInsertProductsAsync(IEnumerable<AmCart.ProductSearch.API.Entities.ProductSearch> products)
        //{
        //    if (products == null || !products.Any())
        //    {
        //        _logger.LogWarning("No products provided for bulk insertion.");
        //        return false;
        //    }

        //    var bulkResponse = await _elasticClient.BulkAsync(b => b
        //        .Index("products")  // Index where the products are stored
        //        .IndexMany(products)  // Bulk indexing products
        //    );

        //    if (!bulkResponse.IsValidResponse)
        //    {
        //        _logger.LogError("Bulk insert failed. Errors: {Errors}", bulkResponse.DebugInformation);
        //        return false;
        //    }

        //    _logger.LogInformation("Successfully inserted {Count} products into Elasticsearch.", products.Count());
        //    return true;
        //}

        ///// <summary>
        ///// Deletes a product from Elasticsearch.
        ///// </summary>
        ///// <param name="productId">ID of the product to delete.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public async Task DeleteProductAsync(string productId)
        //{
        //    var response = await _elasticClient.DeleteAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(productId, d => d
        //        .Index("products")  // Index where the product is stored
        //        .Refresh(Refresh.WaitFor)  // Ensure deletion is visible immediately
        //    );

        //    if (!response.IsValidResponse)
        //    {
        //        _logger.LogError("Failed to delete product {ProductId}. Error: {Error}", productId, response.DebugInformation);
        //    }
        //    else
        //    {
        //        _logger.LogInformation("Product {ProductId} deleted successfully", productId);
        //    }
        //}


        ///// <summary>
        ///// Performs a full-text, fuzzy search and autocomplete for products, prioritizing matches in Name, Category, and Description.
        ///// </summary>
        ///// <param name="query">Search query to use for matching products.</param>
        ///// <returns>A task representing the search response, which contains the matching products.</returns>

        public async Task<SearchResponse<Entities.ProductSearch>> SearchAsync1(string query)
        {


            if (string.IsNullOrWhiteSpace(query))
            {
                return await _elasticClient.SearchAsync<Entities.ProductSearch>(s => s
           .Index("products")
           .Query(q => q.MatchAll(m => { }))
               );
            }

            //if (string.IsNullOrWhiteSpace(query))
            //{
            //    var emptySearchResponse = await _elasticClient.SearchAsync<Entities.ProductSearch>(s => s // Renamed variable
            //        .Index("products")
            //        .Query(q => q.MatchAll(m => m))
            //    );
            //    return emptySearchResponse; // Return the renamed variable
            //}

            var searchResponse = await _elasticClient.SearchAsync<Entities.ProductSearch>(s => s
                .Index("products")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                    // Full-text search with boosting applied to Name, Category, and Description
                    //sh => sh.MultiMatch(m => m
                    //.Fields("Name, Category,Description")

                    // MultiMatch query with specified fields (Name, Category, Description)
                    sh => sh.MultiMatch(m => m
                    //.Fields("name, category,description")

                    //.Fields(new Field("name")
                    //.And(new Field("category"))
                    //.And(new Field("description")))

                    .Fields(new[] { "name", "category", "description" })


                                                             //.Fields(new Fields("name", "category", "description"))  // Specify fields as a collection

                                                             //  .Fields(new Field[]{new Field("name"), new Field("category"), new Field("description") })

                                                             // sh => sh.MultiMatch(m => m
                                                             //.Fields(f => f
                                                             //    .Field("name")        // Search in 'Name' field
                                                             //    .Field("category")    // Search in 'Category' field
                                                             //    .Field("description") // Search in 'Description' field
                                                             //)

                                                             //sh => sh.MultiMatch(m => m
                                                             //      .Fields(f => f
                                                             //          .Field(p => p.Name)        // Search in 'Name' field
                                                             //          .Field(p => p.Category)     // Search in 'Category' field
                                                             //          .Field(p => p.Description)  // Search in 'Description' field
                                                             //      )
                                                             // Full-text search with boosting applied to Name, Category, and Description
                                                             //sh => sh.MultiMatch(m => m
                                                             //.Fields(f => f // Correct Fields usage - finally!
                                                             //    .Field(p => p.Name)
                                                             //    .Field(p => p.Category)
                                                             //    .Field(p => p.Description)
                                                             //))
                                                             .Query(query)
                                //.Fuzziness(Fuzziness.Auto) // Or: Fuzziness.EditDistance(2) for a fixed edit distance
                                //.Fuzziness(Fuzziness.EditDistance(2))
                                //.Fuzziness(new Fuzziness("auto")) // Or new Fuzziness(2)
                                .Fuzziness(new Fuzziness(2)) // Or new Fuzziness(2)
                                .PrefixLength(2)
                                .Boost(2.0f)
                            ),
                            // Autocomplete for Name (highest priority)
                            sh => sh.MatchPhrasePrefix(mpp => mpp
                                .Field(p => p.Name) // Search the analyzed "name" field
                                .Query(query)
                                .Boost(3.0f)
                            ),
                            // Autocomplete for Category (medium priority)
                            sh => sh.MatchPhrasePrefix(mpp => mpp
                                .Field(p => p.Category) // Search the analyzed "category" field
                                .Query(query)
                                .Boost(2.0f)
                            )
                        )
                    )
                )
          //.Sort(s => s // Sort by a combination of score and name.keyword
          //     .Combine(c => c
          //         .Ascending(a => a.Field(f => f.Name.Keyword)) // Sort by name.keyword ascending
          //         .Descending(d => d.Field("_score")) // Sort by score descending
          //     )
          //)
          //.Size(50)


          //.Sort(so => so  // Sort by a combination of score and name.keyword
          //    .Field(f => f.Name.Keyword, fd => fd.Order(SortOrder.Asc)  // Sort by name.keyword ascending
          //    .Field(f => "_score", fd => fd.Order(SortOrder.Desc))     // Sort by score descending
          //)
          //.Size(50) // Limit results to 50 for performance

          //  .Sort(so => so
          //    .Field(f => f.Name.Keyword, fd => fd.Order(SortOrder.Asc)) // Correct!
          //    .Field("_score", fd => fd.Order(SortOrder.Desc))         // Correct!
          //)
          //.Size(50)

          //.Sort(s => s
          //        .Field(f => f.Field("name.keyword").Order(SortOrder.Asc))  // Sort by name.keyword ascending
          //        .Field(f => f.Field("_score").Order(SortOrder.Desc))       // Sort by score descending
          //    )
          //    .Size(50) // Limit results to 50



          //    .Sort(so => so
          //.Field(f => f.Name.Suffix("keyword"), new FieldSort { Order = SortOrder.Asc }) // Sort by name.keyword ascending
          //.Field(f => "_score", new FieldSort { Order = SortOrder.Desc }) // Sort by score descending
          //)
          .Sort(so => so
        .Field(f => f.Name.Suffix("keyword"),  s => s.Order(SortOrder.Asc)) // ✅ Sorting by name.keyword
        .Field("_score", s => s.Order(SortOrder.Desc))                     // ✅ Sorting by relevance score
    )
         .Size(50) // Limit results to 50
                );

            if (!searchResponse.IsSuccess())
            {
                _logger.LogError("Search query failed. Errors: {Errors}", searchResponse.DebugInformation);

                // More robust exception handling (check DebugInformation)
                if (!string.IsNullOrEmpty(searchResponse.DebugInformation))
                {
                    // Log the entire DebugInformation string, which may contain exception details
                    _logger.LogError("Elasticsearch query details: {Details}", searchResponse.DebugInformation);

                    // Attempt to extract exception information (more advanced, might require parsing)
                    // This is highly dependent on the format of the DebugInformation string
                    // and might need to be adapted to your specific error messages.
                    if (searchResponse.DebugInformation.Contains("exception")) // Example: check for "exception" keyword
                    {
                        // You might try to parse the exception details from the string here
                        // This is complex and depends on the format of the error response.
                        // It's often better to just log the entire DebugInformation string.
                        _logger.LogError("Elasticsearch query exception details found in DebugInformation.");
                    }
                }
            }

            return searchResponse;
        }

        ////////////////////  Standard Response ///////////////////////////////////////////////////////////
        ///

        /// <summary>
        /// Performs a full-text, fuzzy search and autocomplete for products using Elasticsearch.
        /// </summary>
        /// <param name="query">The search term.</param>
        /// <returns>A standardized search response containing matching products.</returns>
        /// <summary>
        /// Performs a full-text, fuzzy search and autocomplete for products using Elasticsearch.
        /// </summary>
        /// <param name="query">The search term.</param>
        /// <returns>A standardized search response containing matching products.</returns>
        public async Task<ProductSearchResponse<Entities.ProductSearch>> SearchAsync(string query)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Entities.ProductSearch>(s => s
                    .Index("products")
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                sh => sh.MultiMatch(m => m
                                    .Fields(new[] { "name", "category", "description" })
                                    .Query(query)
                                    .Fuzziness(new Fuzziness(2))
                                    .PrefixLength(2)
                                    .Boost(2.0f)
                                ),
                                sh => sh.MatchPhrasePrefix(mpp => mpp
                                    .Field(p => p.Name)
                                    .Query(query)
                                    .Boost(3.0f)
                                ),
                                sh => sh.MatchPhrasePrefix(mpp => mpp
                                    .Field(p => p.Category)
                                    .Query(query)
                                    .Boost(2.0f)
                                )
                            )
                        )
                    )
                    .Sort(so => so
                        .Field(f => f.Name.Suffix("keyword"), s => s.Order(SortOrder.Asc))
                        .Field("_score", s => s.Order(SortOrder.Desc))
                    )
                    .Size(50)
                );

                if (!searchResponse.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch query failed: {Error}", searchResponse.DebugInformation);
                    return new ProductSearchResponse<Entities.ProductSearch>
                    {
                        Results = new List<Entities.ProductSearch>(),
                        TotalCount = 0,
                        ErrorMessage = "Elasticsearch query failed."
                    };
                }

                // ✅ Return standardized response while keeping your query intact
                return new ProductSearchResponse<Entities.ProductSearch>
                {
                    Results = searchResponse.Documents.ToList(),
                    TotalCount = (int)searchResponse.Total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error occurred during Elasticsearch search for query: {Query}", query);
                return new ProductSearchResponse<Entities.ProductSearch>
                {
                    Results = new List<Entities.ProductSearch>(),
                    TotalCount = 0,
                    ErrorMessage = "An error occurred while processing the search request."
                };
            }
        }




    }
}



///// <summary>
///// Performs a full-text, fuzzy search and autocomplete for products, prioritizing matches in Name, Category, and Description.
///// </summary>
///// <param name="query">Search query to use for matching products.</param>
///// <returns>A task representing the search response, which contains the matching products.</returns>
//public async Task<SearchResponse<Entities.ProductSearch>> SearchAsync(string query)
//{
//    if (string.IsNullOrWhiteSpace(query))
//    {
//        // If query is empty or whitespace, return all products
//        return await _elasticClient.SearchAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(s => s
//            .Index("products")
//            .Query(q => q.MatchAll(m => m))  // Match all products (now requires a parameter)
//        );
//    }

//    var searchResponse = await _elasticClient.SearchAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(s => s
//        .Index("products")
//        .Query(q => q
//            .Bool(b => b
//                .Should(
//                    // Full-text search with boosting applied to Name, Category, and Description
//                    sh => sh.MultiMatch(m => m
//                        .Fields(f => f
//                            .Field(p => p.Name)        // Search in 'Name' field
//                            .Field(p => p.Category)    // Search in 'Category' field
//                            .Field(p => p.Description) // Search in 'Description' field
//                        )
//                        .Query(query)                        // The search query input by the user
//                        //.Fuzziness(Fuzziness.EditDistance(2))  // Use EditDistance fuzziness
//                        .Fuzziness(new Fuzziness(2))
//                        .PrefixLength(2)                    // Require the first 2 characters to match exactly
//                        .Boost(2.0f)                        // Boost this search query (increase relevance score)
//                    ),
//                    // Autocomplete for Name (highest priority)
//                    sh => sh.MatchPhrasePrefix(mpp => mpp
//                        .Field(p => p.Name)
//                        .Query(query)
//                        .Boost(3.0f)  // Highest priority for name match
//                    ),
//                    // Autocomplete for Category (medium priority)
//                    sh => sh.MatchPhrasePrefix(mpp => mpp
//                        .Field(p => p.Category)   // Match the "Category" field
//                        .Query(query)             // The query text entered by the user
//                        .Boost(2.0f)              // Medium priority for category match
//                    )
//                )
//            )
//        )
//        .Sort(s => s
//            .Field(f => f
//                .Field("_score")  // Sort by score (relevance)
//                .Order(SortOrder.Descending)  // Use descending order for relevance
//            )
//        )
//        .Size(50)  // Limit results to 50 for performance
//    );

//    if (!searchResponse.IsValid)
//    {
//        _logger.LogError("Search query failed. Errors: {Errors}", searchResponse.DebugInformation);
//    }

//    return searchResponse;
//}



// -------------------------------------------------



///// <summary>
///// Performs a full-text, fuzzy search and autocomplete for products, prioritizing matches in Name, Category, and Description.
///// </summary>
///// <param name="query">Search query to use for matching products.</param>
///// <returns>A task representing the search response, which contains the matching products.</returns>
//public async Task<SearchResponse<Entities.ProductSearch>> SearchAsync(string query)
//{
//    if (string.IsNullOrWhiteSpace(query))
//    {
//        // If query is empty or whitespace, return all products
//        return await _elasticClient.SearchAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(s => s
//            .Index("products")
//            .Query(q => q.MatchAll())  // Match all products
//        );
//    }

//    var searchResponse = await _elasticClient.SearchAsync<AmCart.ProductSearch.API.Entities.ProductSearch>(s => s
//        .Index("products")
//        .Query(q => q
//            .Bool(b => b
//                .Should(
//                  // Full-text search with boosting applied to Name, Category, and Description
//                      sh => sh.MultiMatch(m => m
//                        .Fields(f => f
//                            .Field(p => p.Name)      // Search in 'Name' field
//                            .Field(p => p.Category)  // Search in 'Category' field
//                            .Field(p => p.Description) // Search in 'Description' field
//                        )
//                        .Query(query)                  // The search query input by the user
//                        .Type(MuQueryType.BestFields)  // Use the correct QueryType enum
//                        .Fuzziness(Elastic.Clients.Elasticsearch.Fuzziness.Auto) // Allow fuzzy matching (Auto adjusts fuzziness)
//                        .PrefixLength(2)              // Require the first 2 characters to match exactly
//                        .Boost(2.0f)                  // Boost this search query (increase relevance score)
//                    ),
//                    // Autocomplete for Name (highest priority)
//                    sh => sh.MatchPhrasePrefix(mpp => mpp
//                        .Field(p => p.Name)
//                        .Query(query)
//                        .Boost(3.0f)  // Highest priority for name match
//                    ),
//                    // Autocomplete for Category (medium priority)
//                    sh => sh.MatchPhrasePrefix(mpp => mpp
//                        .Field(p => p.Category)   // Match the "Category" field
//                        .Query(query)             // The query text entered by the user
//                        .Boost(2.0f)    // Medium priority for category match
//                    )
//                )
//            )
//        )
//        .Sort(s => s
//            .Descending(SortSpecialField.Score)  // Sort by score (relevance)
//        )
//        .Size(50)  // Limit results to 50 for performance
//    );

//    if (!searchResponse.IsValid)
//    {
//        _logger.LogError("Search query failed. Errors: {Errors}", searchResponse.DebugInformation);
//    }

//    return searchResponse;
//}