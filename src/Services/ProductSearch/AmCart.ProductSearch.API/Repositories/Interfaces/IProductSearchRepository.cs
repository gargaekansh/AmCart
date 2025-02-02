

using Elastic.Clients.Elasticsearch;

namespace AmCart.ProductSearch.API.Repositories.Interfaces
{
    public interface IProductSearchRepository
    {
        Task<bool> BulkInsertProductsAsync(IEnumerable<Entities.ProductSearch> products);

        Task DeleteProductAsync(string productId);
        Task InsertOrUpdateProductAsync(Entities.ProductSearch product);
        Task<SearchResponse<Entities.ProductSearch>> SearchAsync(string query);
    }
}
