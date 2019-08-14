using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Data
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string ExternalId { get; set; }

        public string ExternalExtendedResultDescription { get; set; }

        public decimal Amount { get; set; }

        public PaymentDetails PaymentDetails { get; set; }

        public DateTime ProcessDate { get; set; }

        public PaymentStatus Status { get; set; }
    }
}
