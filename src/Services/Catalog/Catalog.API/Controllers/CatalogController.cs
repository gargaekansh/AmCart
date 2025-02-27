﻿using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
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

        [HttpGet("{id:int}", Name = "GetProduct")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _repository.GetProduct(id);
            if (product == null)
            {

                _logger.LogError("Product with id: {ProductId}, not found.", id);
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
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            var createdProduct = await _repository.CreateProduct(product);
            return createdProduct != null ? Ok(createdProduct) : BadRequest();
        }

        [Route("[action]", Name = "PostProductBatch")]
        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> CreateProducts([FromBody] IEnumerable<Product> products)
        {
            bool result = await _repository.CreateProducts(products);
            return result ? Ok(products) : BadRequest();
        }

        [HttpPut]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Product>> UpdateProduct([FromBody] Product product)
        {
            bool result = await _repository.UpdateProduct(product);
            return result ? Ok(product) : NotFound();
        }

        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [Authorize(Roles = "Administrator", Policy = "HasFullAccess")]
        public async Task<IActionResult> DeleteProductById(int id)
        {
            bool result = await _repository.DeleteProduct(id);
            return result ? Ok(result) : NotFound();
        }
    }
}