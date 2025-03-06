namespace AmCart.ProductSearch.API.Shared.Enums
{
    /// <summary>
    /// Represents the types of search providers.
    /// </summary>
    public enum SearchProviderType
    {
        /// <summary>
        /// Indicates an unknown or invalid search provider.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates Elasticsearch as the search provider.
        /// </summary>
        Elasticsearch,

        /// <summary>
        /// Indicates CosmosDb as the search provider.
        /// </summary>
        CosmosDb,

        /// <summary>
        /// Indicates MongoDB as the search provider.
        /// </summary>
        MongoDb
    }
}
