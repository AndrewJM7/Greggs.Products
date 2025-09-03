using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Greggs.Products.Api.Controllers;

/// <summary>
/// Handles requests for product data.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger; 
    private readonly IDataAccess<Product> _productAccess;

     // Exchange rate from GBP to EUR
    private const decimal ExchangeRate = 1.11m;

    /// <summary>
    /// Initialises a new instance of the <see cref="ProductController"/> class.
    /// </summary>
    /// <param name="logger">Diagnostics logger.</param>
    /// <param name="productAccess">Product data access.</param>
    public ProductController(ILogger<ProductController> logger, IDataAccess<Product> productAccess)
    {
        _logger = logger;
        _productAccess = productAccess;
    }

    /// <summary>
    /// Returns products from the menu with optional paging.
    /// </summary>
    /// <param name="pageStart">Index to begin from.</param>
    /// <param name="pageSize">Maximum number of products to return.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a collection of <see cref="Product"/> objects.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get(int? pageStart = 0, int? pageSize = null)
    {
        // Reject invalid parameter values
        var validation = ValidatePaging(pageStart, pageSize);
        if (validation != null) return validation;
        _logger.LogInformation("Fetching products. pageStart={PageStart}, pageSize={PageSize}", pageStart, pageSize);

        // Retrieve products from the data access layer
        var products = _productAccess.List(pageStart, pageSize).ToList();
        _logger.LogInformation("Returned {Count} products", products.Count);
        
        // Return the requested products 
        return Ok(products);
    }

    /// <summary>
    /// Returns products from the menu with prices in both GBP and EUR.
    /// </summary>
    /// <param name="pageStart">Index to begin from.</param>
    /// <param name="pageSize">Maximum number of products to return.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a collection of <see cref="ProductWithEuro"/> objects.</returns>
    [HttpGet("euros")]
    public ActionResult<IEnumerable<ProductWithEuro>> GetWithEuros(int? pageStart = 0, int? pageSize = null)
    {
        // Reject invalid parameter values
        var validation = ValidatePaging(pageStart, pageSize);
        if (validation != null) return validation;
        _logger.LogInformation("Fetching products with EUR prices. pageStart={PageStart}, pageSize={PageSize}", pageStart, pageSize);

        // Retrieve products from the data access layer
        var productsFromDb = _productAccess.List(pageStart, pageSize).ToList();

        // Map to ProductWithEuro DTO (Data Transfer Object)
        var products = productsFromDb.Select(MapToEuro).ToList();
        _logger.LogInformation("Returned {Count} products", products.Count);

        // Return the requested products with EUR prices
        return Ok(products);
    }

    /// <summary>
    /// Validates paging parameters to ensure they are not negative.
    /// </summary>
    /// <param name="pageStart">Page index to begin from.</param>
    /// <param name="pageSize">Maximum number of items to return.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a BadRequest if any parameter is negative, or null if valid.
    /// </returns>
    private ActionResult ValidatePaging(int? pageStart, int? pageSize)
    {
        if (pageStart < 0)
        {
            _logger.LogWarning("Invalid pageStart value requested: {PageStart}", pageStart);
            return BadRequest("pageStart cannot be negative");
        }

        if (pageSize < 0)
        {
            _logger.LogWarning("Invalid pageSize value requested: {PageSize}", pageSize);
            return BadRequest("pageSize cannot be negative");
        }

        return null;
    }

    /// <summary>
    /// Maps a Product to a ProductWithEuro, including EUR price.
    /// </summary>
    /// <param name="product">The product to convert.</param>
    /// <returns>A new ProductWithEuro object with both GBP and EUR prices.</returns>
    private ProductWithEuro MapToEuro(Product product) =>
        new ProductWithEuro
        {
            Name = product.Name,
            PriceInPounds = product.PriceInPounds,
            PriceInEuros = Math.Round(product.PriceInPounds * ExchangeRate, 2, MidpointRounding.AwayFromZero) // Round away from zero for financial precision
        };
}