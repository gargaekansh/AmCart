using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using System;
using System.Threading.Tasks;


namespace AmCart.ProductSearch.API.Entities
{
    /// <summary>
    /// Represents a product stored in the Elasticsearch index.
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
    /// Represents the rating details of a product.
    /// </summary>
    public class ProductSearchRating
    {
        /// <summary>
        /// Gets or sets the average rating value.
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the total count of ratings.
        /// </summary>
        public int Count { get; set; }
    }
}


/// <summary>
/// Creates an Elasticsearch index for ProductSearch with proper mappings if it does not exist.
/// </summary>
/// <param name="client">The Elasticsearch client instance.</param>
/// <param name="indexName">The name of the Elasticsearch index.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public static class ElasticsearchMappings
{
    public static async Task CreateProductSearchIndexAsync(ElasticsearchClient client, string indexName = "products")
    {
        var existsResponse = await client.Indices.ExistsAsync(indexName);
        if (existsResponse.Exists)
        {
            return; // Index already exists, so no need to create it again
        }

        var createIndexRequest = new CreateIndexRequest(indexName)
        {
            Mappings = new TypeMapping
            {
                Properties = new Properties
                {
                    { "id", new KeywordProperty() },
                    { "productId", new IntegerNumberProperty() },
                    { "name", new TextProperty() },
                    { "category", new TextProperty() },
                    { "description", new TextProperty() },
                    { "image", new KeywordProperty() },
                    { "price", new FloatNumberProperty() },
                    { "rating", new ObjectProperty
                        {
                            Properties = new Properties
                            {
                                { "rate", new FloatNumberProperty() },
                                { "count", new IntegerNumberProperty() }
                            }
                        }
                    }
                }
            }
        };


        var createIndexResponse = await client.Indices.CreateAsync(createIndexRequest);

        if (!createIndexResponse.IsSuccess())
        {
            throw new InvalidOperationException($"Failed to create index: {createIndexResponse.ElasticsearchServerError?.Error?.Reason}");
        }
    }
}
