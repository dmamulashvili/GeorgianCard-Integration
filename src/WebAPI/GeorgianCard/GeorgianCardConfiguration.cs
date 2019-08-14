using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.GeorgianCard
{
    public class GeorgianCardConfiguration
    {
        public string PaymentUrl { get; set; }

        public string PaymentSuccessUrl { get; set; }

        public string PaymentFailUrl { get; set; }

        public string MerchantId { get; set; }

        public string PageId { get; set; }

        public string AccountId { get; set; }

        public string Currency { get; set; }

        public string DateTimeFormat { get; set; }

        public string Exponent { get; set; }
    }
}
