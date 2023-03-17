using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _19T1021037.DomainModels;

namespace _19T1021037.Web.Models
{
    public class ProductPhotoSearchOutput
    {
        /// <summary>
        /// Ảnh mặt hàng
        /// </summary>
        public List<ProductPhoto> Photos { get; set; }
    }
}