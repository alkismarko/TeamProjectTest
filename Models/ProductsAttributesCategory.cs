﻿using System;
using System.Collections.Generic;

#nullable disable

namespace TeamProjectTest.Models
{
    public partial class ProductsAttributesCategory
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int AttributeId { get; set; }
        public string AttributeValue { get; set; }
    }
}
