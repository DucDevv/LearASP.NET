using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _19T1021037.DomainModels;

namespace _19T1021037.Web.Models
{
    public class OrderDetailModel
    {
        public Order Data { get; set; }
        public List<OrderDetail> OrderDetail { get; set; }
        public decimal TotalPriceOrder
        {
            get
            {
                decimal SUM = 0;
                foreach (var item in OrderDetail)
                {
                    SUM += item.TotalPrice;
                }
                return SUM;
            }
        }
    }
}