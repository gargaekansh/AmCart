namespace AmCart.ProductSearch.API.Configuration
{
    public class ElasticSearchSettings
    {
        public string Uri { get; set; }
        public string DefaultIndex { get; set; } = "products"; // Default index
        public string Username { get; set; }  // Elasticsearch username
        public string Password { get; set; }  // Elasticsearch password
    }
}
