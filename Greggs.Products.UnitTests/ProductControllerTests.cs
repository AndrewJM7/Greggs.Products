using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IDataAccess<Product>> _mockDataAccess;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            // Set up a mock data access object
            _mockDataAccess = new Mock<IDataAccess<Product>>();
            _controller = new ProductController(Mock.Of<Microsoft.Extensions.Logging.ILogger<ProductController>>(), _mockDataAccess.Object);
        }

        [Fact]
        public void Get_ValidPageStart_ReturnsProducts()
        {
            // Arrange: simulate products from the data access layer
            var fakeProducts = new List<Product>
            {
                new Product { Name = "Sausage Roll", PriceInPounds = 1m },
                new Product { Name = "Vegan Sausage Roll", PriceInPounds = 1.1m }
            };

            _mockDataAccess
                .Setup(d => d.List(It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(fakeProducts);

            // Act: call the controller method
            var result = _controller.Get();

            // Assert: make sure we got an OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            // Assert: confirm we got the products we expected
            Assert.Equal(2, products.Count());
            Assert.Contains(products, p => p.Name == "Sausage Roll");
        }

        [Fact]
        public void GetWithEuros_ReturnsProductsWithEuroPrices()
        {
            // Arrange: one product to test euro conversion
            var fakeProducts = new List<Product>
            {
                new Product { Name = "Sausage Roll", PriceInPounds = 2m }
            };

            _mockDataAccess
                .Setup(d => d.List(It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns(fakeProducts);

            // Act: call the euro endpoint
            var result = _controller.GetWithEuros();

            // Assert: response is OkObjectResult with ProductWithEuro
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<ProductWithEuro>>(okResult.Value);

            // There should be exactly one product
            Assert.Single(products);

            // Assert: EUR price is correctly calculated and rounded
            var product = products.First();
            Assert.Equal(2m, product.PriceInPounds);
            Assert.Equal(Math.Round(2m * 1.11m, 2, MidpointRounding.AwayFromZero), product.PriceInEuros);
        }

        [Theory]
        [InlineData(-1, 5)]
        [InlineData(0, -3)]
        [InlineData(-2, -5)]
        public void Get_NegativePaging_ReturnsBadRequest(int pageStart, int pageSize)
        {
            // Act
            var result = _controller.Get(pageStart, pageSize);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
