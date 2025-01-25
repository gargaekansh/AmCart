using Catalog.API.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        private readonly ILogger<CatalogContext> _logger;
        public CatalogContext(IConfiguration configuration, ILogger<CatalogContext> logger)
        {
            _logger = logger;
            try
            {
                var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

                Products = database.GetCollection<Product>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
               // CatalogContextSeed.SeedData(Products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CatalogContext");
                throw ;
            }
        }

        public IMongoCollection<Product> Products { get; }
    }
}
