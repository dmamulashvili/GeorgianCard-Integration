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
```
