using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _19T1021037.DomainModels;

namespace _19T1021037.Web.Models
{
    public class OrderSearchOutput : PaginationSearchOutput
    {
        public List<Order> Data { get; set; }
    }
}