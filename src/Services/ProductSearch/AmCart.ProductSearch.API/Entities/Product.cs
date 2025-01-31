using DnsClient;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AmCart.ProductSearch.API.Entities
{
    public class Product
    {
        //---------------------------------------------------

        [BsonId] // Indicates that this property is the unique identifier (_id)
        //[BsonRepresentation(BsonType.ObjectId)] // Allows ObjectId to be stored as a string
        public int Id { get; set; } //= ObjectId.GenerateNewId().ToString();

        [BsonElement("name")] // Maps to the "title" field in MongoDB
        public string Name { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("image")]
        public string Image { get; set; }

        [BsonElement("rating")]
        public ProductRating Rating { get; set; }
    }

    public class ProductRating
    {
        [BsonElement("rate")]
        public decimal Rate { get; set; }

        [BsonElement("count")]
        public int Count { get; set; }
    }
}
