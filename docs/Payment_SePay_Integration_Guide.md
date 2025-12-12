# SePay Payment Integration Guide

## Tổng quan
Hệ thống tích hợp SePay để xử lý thanh toán tự động qua chuyển khoản ngân hàng với mã QR.

## Cấu hình

### 1. Web.config Settings

```xml
<!-- SePay Configuration -->
<add key="SePay:BankCode" value="MB" />
<add key="SePay:AccountNumber" value="0359016143" />
<add key="SePay:AccountName" value="BUI DAI PHU" />
<add key="SePay:Template" value="print" />
<add key="SePay:WebhookSecret" value="your-secret-key-here-change-in-production" />
<!-- Comma-separated list of allowed IPs. Leave empty to allow all (development only) -->
<add key="SePay:AllowedIPs" value="103.x.x.x,104.x.x.x" />
```

#### Giải thích các tham số:

- **BankCode**: Mã ngân hàng (MB = MBBank, VCB = Vietcombank, v.v.)
- **AccountNumber**: Số tài khoản nhận tiền
- **AccountName**: Tên chủ tài khoản
- **Template**: Template mã QR (print, compact, v.v.)
- **WebhookSecret**: Khóa bí mật để xác thực webhook (QUAN TRỌNG: Phải thay đổi trong production)
- **AllowedIPs**: Danh sách IP được phép gọi webhook (phân cách bằng dấu phẩy)

### 2. SePay Dashboard Configuration

1. Đăng nhập vào [SePay Dashboard](https://my.sepay.vn)
2. Vào menu **Webhook Settings**
3. Cấu hình webhook URL: `https://your-domain.com/Subscription/SePayWebhook`
4. Copy **Webhook Secret** và cập nhật vào Web.config
5. Lấy danh sách **Webhook IP addresses** và cập nhật vào Web.config

## Cách thức hoạt động

### 1. Flow thanh toán

```
User chọn gói → QR Code được tạo → User quét mã thanh toán 
→ SePay nhận tiền → SePay gọi webhook → Hệ thống kích hoạt gói
```

### 2. Format nội dung chuyển khoản

```
UPGRADE {RecruiterID} {PlanID}
```

Ví dụ:
- `UPGRADE 123 Monthly` - Nâng cấp gói Monthly cho Recruiter ID 123
- `UPGRADE 456 Lifetime` - Nâng cấp gói Lifetime cho Recruiter ID 456

### 3. QR Code Generation

URL format:
```
https://qr.sepay.vn/img?acc={AccountNumber}&bank={BankCode}&amount={Amount}&des={Content}&template={Template}
```

Ví dụ:
```
https://qr.sepay.vn/img?acc=0359016143&bank=MB&amount=25000&des=UPGRADE%20123%20Monthly&template=print
```

## Security Features

### 1. IP Whitelist
Chỉ cho phép webhook từ các IP của SePay:
```csharp
if (!SePaySecurityHelper.IsValidIP(ipAddress))
{
    return Unauthorized;
}
```

### 2. Signature Validation
Xác thực chữ ký HMAC-SHA256:
```csharp
var signature = Request.Headers["X-SePay-Signature"];
if (!SePaySecurityHelper.ValidateSignature(payload, signature))
{
    return Unauthorized;
}
```

### 3. Timestamp Validation
Chống replay attack bằng cách kiểm tra timestamp:
```csharp
if (!SePaySecurityHelper.IsRequestTimestampValid(timestamp, maxAgeMinutes: 10))
{
    return Expired;
}
```

### 4. Idempotency
Tránh xử lý trùng lặp:
```csharp
var existingTx = db.SePayTransactions
    .FirstOrDefault(t => t.ReferenceCode == model.referenceCode);
if (existingTx != null)
{
    return AlreadyProcessed;
}
```

## Webhook Payload

### Request Format

```json
{
    "id": 123456,
    "gateway": "MB",
    "transactionDate": "2025-12-01 14:30:00",
    "accountNumber": "0359016143",
    "subAccount": "",
    "amountIn": 25000,
    "amountOut": 0,
    "accumulated": 100000,
    "code": "TXN123",
    "transactionContent": "UPGRADE 123 Monthly",
    "referenceCode": "REF123456",
    "description": "Payment for subscription"
}
```

### Response Format

Success:
```json
{
    "success": true
}
```

Error:
```json
{
    "success": false,
    "message": "Error description"
}
```

## Logging

### Log Location
```
~/Logs/Payment/payment-{yyyy-MM-dd}.log
```

### Log Levels
- **INFO**: Normal operations
- **WARNING**: Suspicious activities (invalid IP, wrong signature)
- **ERROR**: Exceptions and failures
- **WEBHOOK**: Webhook request/response details
- **WEBHOOK_REQUEST**: Raw webhook request data

### Log Example
```
[2025-12-01 14:30:15.123] [WEBHOOK_REQUEST] Webhook Request:
IP: 103.x.x.x
User-Agent: SePay-Webhook/1.0
Body: {"id":123456,"gateway":"MB",...}

[2025-12-01 14:30:15.456] [INFO] Parsed RecruiterID: 123, PlanID: Monthly

[2025-12-01 14:30:15.789] [INFO] Subscription upgraded successfully. RecruiterID: 123, Old: Free (Expiry: ), New: Monthly (Expiry: 2025-12-31 14:30:15)
```

## Testing

### 1. Development Testing (Bypass Security)

Để test trong môi trường development, để trống `AllowedIPs` và `WebhookSecret`:
```xml
<add key="SePay:AllowedIPs" value="" />
<add key="SePay:WebhookSecret" value="" />
```

### 2. Manual Webhook Test

Sử dụng Postman hoặc curl:

```bash
curl -X POST https://localhost:44300/Subscription/SePayWebhook \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "gateway": "MB",
    "transactionDate": "2025-12-01 14:30:00",
    "accountNumber": "0359016143",
    "amountIn": 25000,
    "transactionContent": "UPGRADE 1 Monthly",
    "referenceCode": "TEST123"
  }'
```

### 3. QR Code Test

1. Mở trình duyệt: `/Subscription/Upgrade?planId=Monthly`
2. Quét mã QR bằng app ngân hàng (môi trường sandbox nếu có)
3. Kiểm tra log file tại `~/Logs/Payment/`

## Troubleshooting

### Webhook không được gọi
1. Kiểm tra URL webhook trong SePay dashboard
2. Kiểm tra domain có accessible từ internet không
3. Kiểm tra firewall/IIS settings
4. Xem log SePay dashboard

### Webhook bị reject
1. Kiểm tra IP có trong whitelist không
2. Kiểm tra signature có đúng không
3. Xem chi tiết trong log file: `~/Logs/Payment/payment-{date}.log`

### Subscription không được kích hoạt
1. Kiểm tra format nội dung chuyển khoản: `UPGRADE {RecruiterID} {PlanID}`
2. Kiểm tra RecruiterID có tồn tại không
3. Kiểm tra số tiền có đủ không
4. Xem log để biết lỗi cụ thể

## Production Checklist

- [ ] Đổi `SePay:WebhookSecret` thành giá trị mạnh
- [ ] Cấu hình `SePay:AllowedIPs` với IP thật của SePay
- [ ] Test webhook với dữ liệu thật
- [ ] Cấu hình HTTPS (bắt buộc)
- [ ] Setup monitoring/alerting cho webhook failures
- [ ] Backup database trước khi deploy
- [ ] Test idempotency (gọi webhook nhiều lần)
- [ ] Setup log rotation cho `~/Logs/Payment/`
- [ ] Cấu hình email notification cho admin khi có lỗi

## Support

- **SePay Support**: support@sepay.vn
- **Documentation**: https://docs.sepay.vn
- **Dashboard**: https://my.sepay.vn

## Changelog

### v1.0.0 (2025-12-01)
- Initial implementation
- IP whitelist validation
- Signature validation
- Idempotency check
- Comprehensive logging
- Configuration moved to Web.config
