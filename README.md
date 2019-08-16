# GeorgianCard-Integration
Simple GeorgianCard Integration with ASP.NET Core WebAPI (w/o Payment Reversal & Recurring payment)

![alt text](https://www.lucidchart.com/publicSegments/view/bb7c8e51-26fe-4d8e-9bff-3dbfb3df74e0/image.png)

## Configuration
1. Configuration model `GeorgianCardConfiguration.cs`:
```
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
```
2. Configuration sub-section in `appsettings.json`:
```
"GeorgianCardConfiguration": {
  "PaymentUrl": "",
  "PaymentSuccessUrl": "",
  "PaymentFailUrl": "",
  "MerchantId": "",
  "PageId": "",
  "AccountId": "",
  "Currency": "981",
  "Exponent": "2",
  "DateTimeFormat": "yyyyMMdd HH:mm:ss"
}
```
3. Registered configuration in `Startup.cs`'s `ConfigureServices` method:
```
services.Configure<GeorgianCard.GeorgianCardConfiguration>(Configuration.GetSection(nameof(GeorgianCard.GeorgianCardConfiguration)));
```
## Integration
1. HTTP GET Request model `GeorgianCardRequest`:
```
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
    public ExtendedResultCode ExtendedResultCode { get; set; }

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
```
2. Place Order implementation `OrdersController.cs`:
```
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

        await _context.SaveChangesAsync();

        var paymentUri = GeorgianCardHelper.BuildPaymentUri(payment.Id.ToString(), _config);

        return Ok(new { RedirectUrl = paymentUri.AbsoluteUri });
    }
}
```
3. Payment Available & Register Payment actions Implementation `GeorgianCardController.cs`:
```
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
            return Ok(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Invalid merch_id."));
        }

        if (!int.TryParse(request.PaymentId, out int paymentId))
        {
            return Ok(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Invalid o.id."));
        }

        var payment = await _context.Payments.FindAsync(paymentId);

        if (payment == null)
        {
            return Ok(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Payment not found."));
        }

        if (string.IsNullOrWhiteSpace(request.TransactionId) || request.TransactionId != payment.ExternalId)
        {
            return Ok(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Fail, "Invalid trx_id."));
        }

        var paymentAmount = GeorgianCardHelper.ConvertGELToGeorgianCardAmount(payment.Amount);

        return Ok(GeorgianCardHelper.BuildPaymentAvailableResponse(ResultCode.Success, ResultCode.Success.ToString(), paymentAmount, _config));
    }

    [HttpGet]
    [Route("register-payment")]
    public async Task<IActionResult> RegisterPayment([FromQuery] GeorgianCardRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MerchantId) || request.MerchantId != _config.MerchantId)
        {
            return Ok(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid merch_id."));
        }

        if (!int.TryParse(request.PaymentId, out int paymentId))
        {
            return Ok(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid o.id."));
        }

        var payment = await _context.Payments.FindAsync(paymentId);

        if (payment == null)
        {
            return Ok(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Payment not found."));
        }

        if (string.IsNullOrWhiteSpace(request.TransactionId) || request.TransactionId != payment.ExternalId)
        {
            return Ok(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid trx_id."));
        }

        if (!DateTime.TryParseExact(request.Date, _config.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime processDate))
        {
            return Ok(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Fail, "Invalid ts."));
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

        return Ok(GeorgianCardHelper.BuildRegisterPaymentResponse(ResultCode.Success, ResultCode.Success.ToString()));
    }
}
```
