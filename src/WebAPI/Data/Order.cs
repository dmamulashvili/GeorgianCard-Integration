using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Data
{
    public class Order
    {
        public int Id { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
