using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

#nullable disable

namespace TeamProjectTest.Models
{
    public partial class Product
    {
        public Product()
        {
            ProductAttributes = new HashSet<ProductAttribute>();
            ShoppingItems = new HashSet<ShoppingItem>();
        }

        public int ProductId { get; set; }
        public string VendorId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public decimal? Price { get; set; }
        public int? ProductStatus { get; set; }
        public int? Rating { get; set; }

        [NotMapped]
        public IFormFile Image { get; set; }

        public virtual AspNetUser Vendor { get; set; }
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; }
        public virtual ICollection<ShoppingItem> ShoppingItems { get; set; }

    }
}
