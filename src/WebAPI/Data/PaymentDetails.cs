using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Data
{
    public class PaymentDetails
    {
        public string AuthorizationCode { get; set; }

        public string Cardholder { get; set; }

        public string ExpirationDate { get; set; }

        public string MaskedPAN { get; set; }

        public string RRN { get; set; }
    }
}
