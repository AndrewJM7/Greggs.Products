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
        // Reject invalid pageStart values
        if (pageStart < 0)
        {
            _logger.LogWarning("Invalid pageStart value requested: {PageStart}", pageStart);
            return BadRequest("pageStart cannot be negative");
        }

         // Log paging parameters used for the request
        _logger.LogInformation("Fetching products. pageStart={PageStart}, pageSize={PageSize}", pageStart, pageSize);

        // Retrieve and log products from the data access layer
        var products = _productAccess.List(pageStart, pageSize).ToList();
        _logger.LogInformation("Returned {Count} products", products.Count);
        
        // Return the requested products 
        return Ok(products);
    }
}