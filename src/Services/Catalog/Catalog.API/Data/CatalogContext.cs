using Catalog.API.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;

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
                string connectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString");
                string databaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName");
                string collectionName = configuration.GetValue<string>("DatabaseSettings:CollectionName");

                _logger.LogInformation($"Connection String: {connectionString}");
                _logger.LogInformation($"Database Name: {databaseName}");
                _logger.LogInformation($"Collection Name: {collectionName}");

                _logger.LogInformation("Connecting to MongoDB...");
                var client = new MongoClient(connectionString);
                _logger.LogInformation("Connected to MongoDB client.");

                _logger.LogInformation("Getting database...");
                var database = client.GetDatabase(databaseName);
                _logger.LogInformation("Database retrieved.");

                _logger.LogInformation("Getting collection...");
                Products = database.GetCollection<Product>(collectionName);
                _logger.LogInformation("Collection retrieved.");
                // CatalogContextSeed.SeedData(Products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CatalogContext");
                throw;
            }
        }

        public IMongoCollection<Product> Products { get; }
    }
}