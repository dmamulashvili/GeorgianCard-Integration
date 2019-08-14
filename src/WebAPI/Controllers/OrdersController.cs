using System;
using System.Collections.Generic;
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
    public class OrdersController : ControllerBase
    {
        private readonly GeorgianCardConfiguration _config;
        private readonly ApplicationDbContext _context;

        public OrdersController(IOptions<GeorgianCardConfiguration> options
            //, ApplicationDbContext context
            )
        {
            _config = options.Value;
            //_context = context;
        }

        [HttpPut]
        [Route("{id:int}/place")]
        public async Task<IActionResult> PlaceOrder(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalPrice,
                Status = PaymentStatus.Pending
            };
            await _context.AddAsync(payment);

            order.Status = OrderStatus.PendingPayment;

            _context.Update(order);

            await _context.SaveChangesAsync();

            var paymentUri = GeorgianCardHelper.BuildPaymentUri(payment.Id.ToString(), _config);

            return Ok(new { paymentRedirectUrl = paymentUri.AbsoluteUri });
        }
    }
}