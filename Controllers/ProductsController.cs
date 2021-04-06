using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TeamProjectTest.Authentication;
using TeamProjectTest.Models;
using TeamProjectTest.UserServices;
using TeamProjectTest.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamProjectTest.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly TeamProjectContext _context;

        public static IWebHostEnvironment _webHostEnvironment;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUserService _userService;

        private readonly ILogger<ProductsController> _logger;

        public ProductsController(TeamProjectContext context, UserManager<ApplicationUser> userManager,
            IUserService userService, ILogger<ProductsController> log)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
            _logger = log;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: ALL PRODUCTS
        //[Authorize(Roles = UserRoles.Vendor)] //Με αυτό τον τρόπο όταν θέλω 1 role
        //[Authorize(Roles = "Vendor,Admin")] //Με αυτό τον τρόπο γράφω όταν θέλω multiple roles 
        [HttpGet("GetAllProducts")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<IEnumerable<ProductsAll>>> GetAllProducts()
        {
            var allProducts = await _context.ProductsAlls.ToListAsync();

            string json = JsonConvert.SerializeObject(allProducts); //Log in row
            string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented); //Log as Json

            _logger.LogInformation(jsonFormatted);

            return allProducts;
        }


        //GET: PRODUCT BY ID
        [Route("GetByProductId/{productid:int}/")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsByProductId>>> GetByProductId(int productid)
        {

            var product = await _context.ProductsByProductIds.Where(p => p.ProductId == productid).ToListAsync();
            var result = _context.ProductsByProductIds.FirstOrDefault(x => x.ProductId == productid);

            if (result == null)
            {
                _logger.LogInformation($"User clicked API GetByProductId with the id {productid} but we do not have this productId in our database");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "This product id does not exist!" });
            }

            string json = JsonConvert.SerializeObject(product); //Log in row
            string jsonFormatted = JValue.Parse(json).ToString(Formatting.Indented); //Log as Json

            _logger.LogInformation(jsonFormatted);

            return product;
        }

        // GET: ALL PRODUCTS BY VENDOR
        [Authorize(Roles = "Vendor,Admin")] //Με αυτό τον τρόπο γράφω όταν θέλω multiple roles 
        [Route("GetProductsByVendor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsByVendorId>>> GetProductsByVendor()
        {

            var user = User.Claims.FirstOrDefault(v => v.Type == ClaimTypes.NameIdentifier).Value;

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "You need to log in to see your products" });
            }

            return await _context.ProductsByVendorIds.Where(u => u.VendorId == user).ToListAsync();
        }


        //GET: ALL PRODUCTS BY CATEGORYID
        //  [Authorize(Roles = "Vendor,Admin,Anonymous,User")] //Με αυτό τον τρόπο γράφω όταν θέλω multiple roles 
        [HttpGet("GetProductsByCategoryId/{categoryId:int}/")]
        public async Task<ActionResult<IEnumerable<ProductsByCategoryId>>> GetProductsByCategoryId(int categoryId)
        {
            var result = _context.ProductsByCategoryIds.FirstOrDefault(x => x.CategoryId == categoryId);

            if (result == null)
            {
                _logger.LogInformation($"User clicked API GetProductsByCategory with the id {categoryId} but we do not have this categoryId in our database");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "This category id does not exist!" });
            }

            return await _context.ProductsByCategoryIds.Where(p => p.CategoryId == categoryId).ToListAsync();
        }

        //GET: ALL PRODUCTS BY ATTRIBUTES
        [Route("ProductsByAttributeIds")]
        [HttpPost]
        public IEnumerable<ProductsAttributesCategory> GetCategoryProducts(int categoryId, ICollection<int> attributeIds)
        {

            return from p in _context.ProductsAttributesCategories
                   where p.CategoryId.Equals(categoryId) || attributeIds.Contains(p.AttributeId)
                   select p;
        }

        //GET: TOP FOUR PRODUCTS
        [Route("GetTopFourProducts")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsTop>>> GetTopProducts()
        {
            return await _context.ProductsTops.Where(p => p.Price < 501).ToListAsync();
        }


        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Vendor,Admin")]
        [HttpPost]
        public ActionResult Product([FromForm] Product prd) //Prepei na onomasw tin methodo mou Product gia na pernaei to product stin vasi
        {
            var user = User.Claims.FirstOrDefault(v => v.Type == ClaimTypes.NameIdentifier).Value;


            string vendorId = user;
            string productName = prd.ProductName;
            string sku = prd.Sku;
            decimal? price = prd.Price;
            int? productStatus = prd.ProductStatus;
            int? rating = prd.Rating;
            var image = prd.Image;

            if (image.Length > 0)
            {
                var imagePath = @"wwwroot\Images\";
                var uploadPath = _webHostEnvironment + imagePath;

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var uniqFileName = sku;
                var filename = Path.GetFileName(uniqFileName + "." + image.FileName.Split(".")[1].ToLower());
                string fullPath = uploadPath + filename; //name with only SKU

                imagePath = imagePath + @"\";
                var filePath = @".." + Path.Combine(imagePath, filename);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                _context.Products.Add(prd);
                _context.SaveChangesAsync();
            }


            return Ok(new { status = true, message = "Added Successfully " + prd.ProductId + " " + prd.ProductName });
        }

        //Update Product by ID
        [Authorize(Roles = "Vendor,Admin")]
        [HttpPut("UpdateProductById")]
        public async Task<IActionResult> UpdateProductById(int productid, [FromForm] Product product)
        {
            if (productid != product.ProductId)
            {
                return BadRequest();
            }
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                string vendorId = product.VendorId;
                string productName = product.ProductName;
                string sku = product.Sku;
                decimal? price = product.Price;
                int? productStatus = product.ProductStatus;
                decimal? rating = product.Rating;
                var image = product.Image;
                if (image.Length > 0)
                {
                    var imagePath = @"wwwroot\Images\";
                    var uploadPath = _webHostEnvironment + imagePath;

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var uniqFileName = sku;
                    var filename = Path.GetFileName(uniqFileName + "." + image.FileName.Split(".")[1].ToLower());
                    string fullPath = uploadPath + filename; //name with only SKU

                    imagePath = imagePath + @"\";
                    var filePath = @".." + Path.Combine(imagePath, filename);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }
                }
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(productid))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "This product id does not exist!" });
                }
                else
                {
                    throw;
                }
            }
            return Ok(new { status = true, message = "Update Successfully" });
        }


        // DELETE Product
        [Authorize(Roles = "Vendor,Admin")]
        [HttpDelete("DeleteProduct/{productid:int}")]
        public async Task<IActionResult> DeleteProduct(int productid)
        {
            var product = await _context.Products.FindAsync(productid);
            if (product == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "This product id does not exist!" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();


            return Ok(new { status = true, message = "Delete Successfully" });
        }

        private bool ProductExists(int productid)
        {
            return _context.Products.Any(p => p.ProductId == productid);
        }

    }
}
