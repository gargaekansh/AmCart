using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    //[Authorize(Policy = "CanRead")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpGet("testroles")]
        public IActionResult TestRoles()
        {
            var user = User.Identity;
            var roles = User.Claims.Where(c => c.Type == JwtClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                UserName = user?.Name,
                Roles = roles,
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }) // Debug all claims
            });
        }


        [AllowAnonymous]
        [HttpGet("test")]
        public ActionResult<string> TestEndpoint()
        {
            return "Test endpoint is working";
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _repository.GetProducts();
            return Ok(products);
        }

        [HttpGet("{id}", Name = "GetProduct")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProductById(string id)
        {
            if (!int.TryParse(id, out int productId))
            {
                _logger.LogError("Invalid product ID: {ProductId}. ID must be an integer.", id);
                return BadRequest("Product ID must be an integer.");
            }

            var product = await _repository.GetProduct(productId);
            if (product == null)
            {
                _logger.LogError("Product with id: {ProductId}, not found.", productId);
                return NotFound();
            }

            return Ok(product);
        }

        [Route("[action]/{category}", Name = "GetProductByCategory")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(string category)
        {
            var products = await _repository.GetProductByCategory(category);
            return Ok(products);
        }

        [Route("[action]", Name = "PostProduct")]
        [HttpPost]
        [Authorize(Policy = "HasFullAccess")]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            var createdProduct = await _repository.CreateProduct(product);
            return createdProduct != null ? Ok(createdProduct) : BadRequest();
        }

        [Route("[action]", Name = "PostProductBatch")]
        [HttpPost]
        [Authorize(Policy = "HasFullAccess")]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> CreateProducts([FromBody] IEnumerable<Product> products)
        {
            bool result = await _repository.CreateProducts(products);
            return result ? Ok(products) : BadRequest();
        }

        [HttpPut]
        [Authorize(Policy = "HasFullAccess")]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Product>> UpdateProduct([FromBody] Product product)
        {
            bool result = await _repository.UpdateProduct(product);
            return result ? Ok(product) : NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteProduct")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [Authorize(Policy = "HasFullAccess")]
        public async Task<IActionResult> DeleteProductById(string id)
        {
            if (!int.TryParse(id, out int productId))
            {
                _logger.LogError("Invalid product ID: {ProductId}. ID must be an integer.", id);
                return BadRequest("Product ID must be an integer.");
            }

            bool result = await _repository.DeleteProduct(productId);
            return result ? Ok(result) : NotFound();
        }

        //[HttpDelete("{id:int}", Name = "DeleteProduct")]
        //[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        ////[Authorize(Roles = "Administrator", Policy = "HasFullAccess")]
        //[Authorize(Policy = "HasFullAccess")]
        //public async Task<IActionResult> DeleteProductById(int id)
        //{
        //    bool result = await _repository.DeleteProduct(id);
        //    return result ? Ok(result) : NotFound();
        //}
    }
}