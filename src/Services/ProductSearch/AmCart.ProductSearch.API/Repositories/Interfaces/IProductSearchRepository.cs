

using AmCart.ProductSearch.API.DTO;
using AmCart.ProductSearch.API.Entities;
using Elastic.Clients.Elasticsearch;

namespace AmCart.ProductSearch.API.Repositories.Interfaces
{
    public interface IProductSearchRepository
    {
        //Task<bool> BulkInsertProductsAsync(IEnumerable<Entities.ProductSearch> products);

        //Task DeleteProductAsync(string productId);
        //Task InsertOrUpdateProductAsync(Entities.ProductSearch product);
        //Task<SearchResponse<Entities.ProductSearch>> SearchAsync(string query);

         Task<ProductSearchResponse<Entities.ProductSearch>> SearchAsync(string query);
         //Task<ProductSearchResponse<Product>> SearchAsync(string query);

        // async Task<ProductSearchResponse<Product>> SearchAsync(string query)
    }
}
