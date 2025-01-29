namespace AmCart.ProductSearch.API.Configuration
{
    public class ElasticSearchSettings
    {
        public string Uri { get; set; }
        public string DefaultIndex { get; set; } = "products"; // Default index
    }
}
