using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAPI.Data;
using WebAPI.GeorgianCard;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeorgianCardController : ControllerBase
    {
        private readonly GeorgianCardConfiguration _config;
        private readonly ApplicationDbContext _context;

        public GeorgianCardController(IOptions<GeorgianCardConfiguration> options
            //, ApplicationDbContext context
            )
        {
            _config = options.Value;
            //_context = context;
        }

        [HttpGet]
        [Route("payment-available")]
        public async Task<IActionResult> PaymentAvailable([FromQuery] GeorgianCardRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MerchantId) || request.MerchantId != _config.MerchantId)
            {
                return Content(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Invalid merch_id."));
            }

            if (!int.TryParse(request.PaymentId, out int paymentId))
            {
                return Content(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Invalid o.id."));
            }

            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                return Content(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Payment not found."));
            }

            if (string.IsNullOrWhiteSpace(request.TransactionId) || request.TransactionId != payment.ExternalId)
            {
                return Content(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Invalid trx_id."));
            }

            var paymentAmount = GeorgianCardHelper.ConvertGELToGeorgianCardAmount(payment.Amount);

            return Content(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Success, ResultCode.Success.ToString(), paymentAmount, _config));
        }

        [HttpGet]
        [Route("register-payment")]
        public async Task<IActionResult> RegisterPayment([FromQuery] GeorgianCardRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MerchantId) || request.MerchantId != _config.MerchantId)
            {
                return Content(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid merch_id."));
            }

            if (!int.TryParse(request.PaymentId, out int paymentId))
            {
                return Content(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid o.id."));
            }

            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                return Content(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Payment not found."));
            }

            if (string.IsNullOrWhiteSpace(request.TransactionId) || request.TransactionId != payment.ExternalId)
            {
                return Content(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid trx_id."));
            }

            if (!DateTime.TryParseExact(request.Date, _config.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime processDate))
            {
                return Content(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid ts."));
            }

            payment.ProcessDate = processDate;

            var order = await _context.Orders.FindAsync(payment.OrderId);

            if (request.ResultCode == ResultCode.Success)
            {
                payment.Status = PaymentStatus.Succeeded;

                payment.PaymentDetails = new PaymentDetails
                {
                    AuthorizationCode = request.AuthCode,
                    Cardholder = request.Cardholder,
                    ExpirationDate = request.ExpiryDate,
                    MaskedPAN = request.MaskedPan,
                    RRN = request.RRN
                };

                // order.Status = OrderStatus.Processing;
                // NOTE: All product orders require processing, except those that only contain products which are both Virtual and Downloadable.

                order.Status = OrderStatus.Completed;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;

                order.Status = OrderStatus.Failed;
            }

            if (Enum.TryParse(request.ExtendedResultCode, true, out ExtendedResultCode extendedResultCode))
            {
                payment.ExternalExtendedResultDescription = extendedResultCode.ToString();
            }

            await _context.SaveChangesAsync();

            return Content(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Success, ResultCode.Success.ToString()));
        }
    }
}