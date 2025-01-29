using Nest;

namespace AmCart.ProductSearch.API.Entities
{
    public class ProductSearch
    {
        [Text(Name = "id")]
        public string Id { get; set; }

        [Text(Name = "name")]
        public string Name { get; set; }

        [Text(Name = "category")]
        public string Category { get; set; }

        [Text(Name = "description")]
        public string Description { get; set; }

        [Keyword(Name = "image")]
        public string Image { get; set; }

        [Number(NumberType.Double)]
        public decimal Price { get; set; }

        [Object(Name = "rating")]
        public Rating Rating { get; set; }
    }

    public class Rating
    {
        [Number(NumberType.Float)]
        public decimal Rate { get; set; }

        [Number(Name = "count")]
        public int Count { get; set; }
    }
}
