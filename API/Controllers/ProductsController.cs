using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet.Models;

namespace Projet.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class ProductsController : ControllerBase
    {
        [Authorize(Policy = "Can_View_Product",  AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public IActionResult GetProducts()
        {
            return Ok(GetProductsList());
        }

        [Authorize(Policy = "Can_Add_Product",  AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            return Ok(GetProductsList().FirstOrDefault(x => x.ProductID == id));
        }

        [Authorize(Policy = "Can_Add_Product",  AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public IActionResult PostProduct([FromBody] Product product)
        {
            using var db = new Northwind();
            db.Products.Add(product);
            db.SaveChanges();
            return Ok(product as Product);
        }

        [Authorize(Policy = "Can_Add_Product",  AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public IActionResult PostProducts([FromBody] List<Product> products)
        {
            using var db = new Northwind();
            db.Products.AddRange(products);
            db.SaveChanges();
            return Ok(products as List<Product>);
        }

        [Authorize(Policy = "Can_Add_Product",  AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public IActionResult DeleteProduct([FromBody] string prod)
        {
            using var db = new Northwind();

            Product product = db.Products.FirstOrDefault(p => p.ProductName == prod);

            db.Remove(product);
            db.SaveChanges();

            return Ok("Product deleted successfully");
        }

        [NonAction]
        private static List<Product> GetProductsList()
        {
            using var db = new Northwind();
            List<Product> prods = db.Products.ToList();

            return prods;
        }
    }
}

