namespace Greggs.Products.Api.Models;

public class Product
{
    public string Name { get; set; }
    public decimal PriceInPounds { get; set; }
}

/// <summary>
/// A product with both GBP and EUR prices.
/// </summary>
public class ProductWithEuro
{
    public string Name { get; set; }
    public decimal PriceInPounds { get; set; }
    public decimal PriceInEuros { get; set; }
}