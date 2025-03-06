using AmCart.ProductSearch.API.Repositories.Interfaces;
using AmCart.ProductSearch.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AmCart.ProductSearch.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductSearchController : ControllerBase
    {
        private readonly IProductSearchRepository _productSearchRepository;
        private readonly ILogger<ProductSearchController> _logger;

        public ProductSearchController(IProductSearchRepository productSearchRepository, ILogger<ProductSearchController> logger)
        {
            _productSearchRepository = productSearchRepository ?? throw new ArgumentNullException(nameof(productSearchRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Search for products based on a query string.
        /// </summary>
        /// <param name="query">The search query string to find relevant products.</param>
        /// <returns>A list of products matching the search query.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Entities.ProductSearch>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("❌ Search query is empty or null.");
                return BadRequest("Query parameter is required.");
            }

            try
            {
                _logger.LogInformation("🔍 Search initiated for query: {Query}", query);
                var searchResponse = await _productSearchRepository.SearchAsync(query);

                // ✅ Handle errors from Elasticsearch or CosmosDB
                if (!string.IsNullOrEmpty(searchResponse.ErrorMessage))
                {
                    _logger.LogError("❌ Search failed: {Error}", searchResponse.ErrorMessage);
                    return StatusCode((int)HttpStatusCode.InternalServerError, searchResponse.ErrorMessage);
                }

                // ✅ Return NoContent if no products are found
                if (searchResponse.Results == null || searchResponse.Results.Count == 0)
                {
                    _logger.LogInformation("🛑 No products found for query: {Query}", query);
                    return NoContent();
                }

                _logger.LogInformation("✅ Search successful. Found {Count} products.", searchResponse.TotalCount);
                return Ok(searchResponse.Results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ An error occurred while processing the search for query: {Query}", query);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
