using AmCart.ProductSearch.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        // Constructor to inject dependencies and ensure they are not null
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
        //[HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AmCart.ProductSearch.API.Entities.ProductSearch>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)] // Handles bad request errors
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)] // Handles internal server errors
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query)
        {
            // Validate query parameter
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Search query is empty or null.");
                return BadRequest("Query parameter is required.");
            }

            try
            {
                _logger.LogInformation("Search initiated for query: {Query}", query);
                var searchResult = await _productSearchRepository.SearchAsync(query);

                // If no results are found, return 204 No Content
                if (searchResult.Documents == null || !searchResult.Documents.Any())
                {
                    _logger.LogInformation("No products found for query: {Query}", query);
                    return NoContent(); // No content if no results
                }

                _logger.LogInformation("Search successful, found {Count} products.", searchResult.Documents.Count());
                return Ok(searchResult.Documents); // Return results as OK
            }
            catch (Exception ex)
            {
                // Log the error and return an InternalServerError response
                _logger.LogError(ex, "An error occurred while processing the search for query: {Query}", query);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
