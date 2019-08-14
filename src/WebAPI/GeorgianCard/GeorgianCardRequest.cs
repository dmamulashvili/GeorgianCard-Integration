using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.GeorgianCard
{
    public class GeorgianCardRequest
    {
        [BindProperty(Name = "merch_id")]
        public string MerchantId { get; set; }

        [BindProperty(Name = "trx_id")]
        public string TransactionId { get; set; }

        [BindProperty(Name = "o.id")]
        public string PaymentId { get; set; }

        [BindProperty(Name = "ts")]
        public string Date { get; set; }

        [BindProperty(Name = "result_code")]
        public ResultCode ResultCode { get; set; }

        [BindProperty(Name = "ext_result_code")]
        public string ExtendedResultCode { get; set; }

        [BindProperty(Name = "p.authcode")]
        public string AuthCode { get; set; }

        [BindProperty(Name = "p.expiryDate")]
        public string ExpiryDate { get; set; }

        [BindProperty(Name = "p.cardholder")]
        public string Cardholder { get; set; }

        [BindProperty(Name = "p.maskedPan")]
        public string MaskedPan { get; set; }

        [BindProperty(Name = "p.rrn")]
        public string RRN { get; set; }
    }
}
