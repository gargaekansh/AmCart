namespace AmCart.ProductSearch.API.DTO
{
    /// <summary>
    /// Standardized response format for product search results.
    /// </summary>
    /// <typeparam name="T">The type of product search entity.</typeparam>
    public class ProductSearchResponse<T>
    {
        /// <summary>
        /// The list of matching products.
        /// </summary>
        public List<T> Results { get; set; } = new();

        /// <summary>
        /// The total count of matching records.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Optional error message (only populated if an error occurs).
        /// </summary>
        public string? ErrorMessage { get; set; } // ✅ Added Error property for consistency
    }


}
