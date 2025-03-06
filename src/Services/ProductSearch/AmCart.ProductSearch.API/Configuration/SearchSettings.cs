using AmCart.ProductSearch.API.Shared.Enums;
using System;

namespace AmCart.ProductSearch.API.Configuration
{
    /// <summary>
    /// Represents the configuration settings for the search provider.
    /// This class determines the search provider type based on configuration values.
    /// </summary>
    public class SearchSettings
    {
        private string _searchProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchSettings"/> class.
        /// Required for dependency injection and configuration binding.
        /// </summary>
        public SearchSettings() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchSettings"/> class with a specified search provider.
        /// </summary>
        /// <param name="searchProvider">The name of the search provider (e.g., "Elasticsearch", "CosmosDb", "MongoDB").</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="searchProvider"/> is null or empty.</exception>
        public SearchSettings(string searchProvider)
        {
            SetSearchProvider(searchProvider);
        }

        /// <summary>
        /// Gets or sets the search provider name.
        /// This property is used for binding from configuration files.
        /// </summary>
        public string SearchProvider
        {
            get => _searchProvider;
            set => SetSearchProvider(value);
        }

        /// <summary>
        /// Gets the type of the search provider.
        /// This property is derived based on the <see cref="SearchProvider"/> value.
        /// </summary>
        public SearchProviderType ProviderType { get; private set; }

        /// <summary>
        /// Sets the search provider name and determines its corresponding type.
        /// </summary>
        /// <param name="searchProvider">The name of the search provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="searchProvider"/> is null or empty.</exception>
        private void SetSearchProvider(string searchProvider)
        {
            _searchProvider = searchProvider ?? throw new ArgumentNullException(nameof(searchProvider));

            ProviderType = searchProvider switch
            {
                "Elasticsearch" => SearchProviderType.Elasticsearch,
                "CosmosDb" => SearchProviderType.CosmosDb,
                "MongoDB" => SearchProviderType.MongoDb,
                _ => SearchProviderType.Unknown
            };
        }
    }
}
