using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic;

namespace AmCart.ProductSearch.API.Entities
{
    /// <summary>
    /// Represents a product in the search index.
    /// </summary>
    public class ProductSearch
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category of the product.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the description of the product.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image URL for the product.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the rating information for the product.
        /// </summary>
        public ProductSearchRating Rating { get; set; }
    }

    /// <summary>
    /// Represents the rating information for a product.
    /// </summary>
    public class ProductSearchRating
    {
        /// <summary>
        /// Gets or sets the average rating of the product.
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the number of ratings for the product.
        /// </summary>
        public int Count { get; set; }
    }
}

public static class ElasticsearchMappings
{
    /// <summary>
    /// Creates the ProductSearch index and its mappings in Elasticsearch if it does not exist.
    /// </summary>
    /// <param name="client">The Elasticsearch client.</param>
    /// <param name="indexName">The name of the index to create (default is "products").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CreateProductSearchIndexAsync(ElasticClient client, string indexName = "products")
    {
        // Check if the index exists
        var existsResponse = await client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            return; // Index already exists, no need to create again
        }

        // Create the index with mappings
        var createIndexResponse = await client.Indices.CreateAsync(indexName, c => c
            .Map<ProductSearch>(m => m
                .Properties(p => p
                    .Text(t => t.Name(n => n.Id))
                    .Number(n => n.Name(p => p.ProductId).Type(NumberType.Integer))
                    .Text(t => t.Name(p => p.Name))
                    .Text(t => t.Name(p => p.Category))
                    .Text(t => t.Name(p => p.Description))
                    .Keyword(k => k.Name(p => p.Image))
                    .Number(n => n.Name(p => p.Price).Type(NumberType.Float))
                    .Object<ProductSearchRating>(o => o.Name(p => p.Rating).Properties(rp => rp
                        .Number(n => n.Name(r => r.Rate).Type(NumberType.Float))
                        .Number(n => n.Name(r => r.Count).Type(NumberType.Integer))
                    ))
                )
            )
        );

        if (!createIndexResponse.IsValid)
        {
            throw new InvalidOperationException("Failed to create index or mappings: " + createIndexResponse.DebugInformation);
        }
    }
}
