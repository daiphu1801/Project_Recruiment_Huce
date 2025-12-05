# Payment System Implementation - Complete

## 🎯 Solution: VietQR.io API + SePay Webhook

**Tại sao chọn giải pháp này?**
- ✅ **Chi phí**: 0-500k/tháng (rẻ nhất)
- ✅ **Professional QR**: VietQR.io chuẩn NAPAS
- ✅ **Tự động 100%**: SePay webhook
- ✅ **Không cần GPKD**: Phù hợp startup/MVP
- ✅ **Setup nhanh**: 30 phút

**So với các giải pháp khác:**
- VNPay/Momo: Cần GPKD + 1-2% phí giao dịch
- SePay link only: QR đơn giản, ít professional
- PayOS: Tốt nhưng có phí transaction

👉 **Xem chi tiết**: [Payment_Solutions_Comparison.md](Payment_Solutions_Comparison.md)

---

## 📋 Quick Start

### 1. Đăng ký VietQR.io (5 phút)
```
→ https://my.vietqr.io/
→ Copy Client ID & API Key
```

### 2. Cấu hình Web.config
```xml
<add key="VietQR:ClientId" value="YOUR_CLIENT_ID" />
<add key="VietQR:ApiKey" value="YOUR_API_KEY" />
<add key="Payment:AccountNumber" value="0812956789" />
<add key="Payment:BankBIN" value="970422" />
```

### 3. Đăng ký SePay Webhook
```
→ https://sepay.vn/
→ Webhook URL: https://your-domain.com/Subscription/SePayWebhook
```

### 4. Test
```
→ /Subscription/Upgrade?planId=Monthly
→ Quét QR và thanh toán
→ Check log: ~/Logs/Payment/
```

👉 **Hướng dẫn chi tiết**: [Payment_VietQR_SePay_Setup_Guide.md](Payment_VietQR_SePay_Setup_Guide.md)

---

## 🏗️ Architecture

```
User → VietQR QR Code → Bank App → Transfer
                                      ↓
                               SePay detects
                                      ↓
                            Webhook → Controller
                                      ↓
                          Auto activate subscription
```

**Components**:
1. **VietQRService**: Generate professional QR codes
2. **SubscriptionController**: Handle upgrade & webhook
3. **SePaySecurityHelper**: IP whitelist, signature validation
4. **PaymentLogger**: Comprehensive logging

---

## ✅ Features Implemented

### 1. VietQR.io Integration
- **Trước**: Hard-code constants trong Controller
- **Sau**: Tất cả cấu hình được move vào `Web.config`
- **Files thay đổi**: 
  - `Web.config` - Thêm section SePay configuration
  - `SubscriptionController.cs` - Đọc config từ ConfigurationManager

```xml
<add key="SePay:BankCode" value="MB" />
<add key="SePay:AccountNumber" value="0359016143" />
<add key="SePay:AccountName" value="BUI DAI PHU" />
<add key="SePay:Template" value="print" />
<add key="SePay:WebhookSecret" value="your-secret-key-here" />
<add key="SePay:AllowedIPs" value="" />
```

### ✅ 2. Webhook Security
Đã implement 4 lớp bảo mật:

#### a) IP Whitelist Validation
```csharp
if (!SePaySecurityHelper.IsValidIP(ipAddress))
{
    PaymentLogger.Warning($"Blocked unauthorized IP: {ipAddress}");
    return Unauthorized;
}
```

#### b) HMAC-SHA256 Signature Validation
```csharp
var signature = Request.Headers["X-SePay-Signature"];
if (!SePaySecurityHelper.ValidateSignature(payload, signature))
{
    return InvalidSignature;
}
```

#### c) Timestamp Validation (Anti-Replay Attack)
```csharp
if (!SePaySecurityHelper.IsRequestTimestampValid(timestamp, maxAgeMinutes: 10))
{
    return RequestExpired;
}
```

#### d) User-Agent Logging
Log tất cả request headers để audit trail

**Files mới**: `Helpers/SePaySecurityHelper.cs`

### ✅ 3. Idempotency Check
Ngăn chặn xử lý trùng lặp khi webhook được gọi nhiều lần:

```csharp
var existingTx = db.SePayTransactions
    .FirstOrDefault(t => t.ReferenceCode == model.referenceCode);
if (existingTx != null)
{
    PaymentLogger.Info("Duplicate webhook - already processed");
    return Json(new { success = true, message = "Already processed" });
}
```

### ✅ 4. Comprehensive Logging
Logging đầy đủ cho mọi operation:

#### Log Levels:
- **INFO**: Normal operations (QR generated, transaction processed, subscription upgraded)
- **WARNING**: Security issues (invalid IP, wrong signature, blocked requests)
- **ERROR**: Exceptions with full stack trace
- **WEBHOOK**: Webhook payload details
- **WEBHOOK_REQUEST**: Raw request data (IP, headers, body)

#### Log Location:
```
~/Logs/Payment/payment-{yyyy-MM-dd}.log
```

#### Example Log Output:
```
[2025-12-01 14:30:15.123] [WEBHOOK_REQUEST] Webhook Request:
IP: 103.x.x.x
User-Agent: SePay-Webhook/1.0
Body: {"id":123456,"amountIn":25000,...}

[2025-12-01 14:30:15.456] [INFO] Parsed RecruiterID: 123, PlanID: Monthly

[2025-12-01 14:30:15.789] [INFO] Subscription upgraded successfully. 
RecruiterID: 123, Old: Free, New: Monthly (Expiry: 2025-12-31)
```

**Files mới**: `Helpers/PaymentLogger.cs`

## 📁 Files Structure

### New Files:
1. **Services/VietQRService.cs** - VietQR.io API integration ⭐
2. **Helpers/PaymentLogger.cs** - Comprehensive logging
3. **Helpers/SePaySecurityHelper.cs** - Security validation
4. **docs/Payment_VietQR_SePay_Setup_Guide.md** - Complete setup guide ⭐
5. **docs/Payment_Solutions_Comparison.md** - Solutions comparison ⭐
6. **Logs/Payment/** - Payment logs directory

### Modified Files:
7. **Web.config** - VietQR & SePay configuration
8. **Controllers/SubscriptionController.cs** - VietQR + Security + Logging
9. **Views/Subscription/Upgrade.cshtml** - Enhanced UI with better QR display

### Legacy (Replaced):
- ~~SePay QR Link~~ → VietQR.io API ✅

## Cách sử dụng

### Development Environment
```xml
<!-- Allow all IPs, no signature check -->
<add key="SePay:AllowedIPs" value="" />
<add key="SePay:WebhookSecret" value="" />
```

### Production Environment
```xml
<!-- Restrict to SePay IPs only -->
<add key="SePay:AllowedIPs" value="103.x.x.x,104.x.x.x" />
<add key="SePay:WebhookSecret" value="strong-random-secret-key" />
```

## Testing

### 1. Manual Webhook Test (Postman/curl)
```bash
curl -X POST https://localhost/Subscription/SePayWebhook \
  -H "Content-Type: application/json" \
  -H "X-SePay-Signature: abc123..." \
  -d '{"amountIn":25000,"transactionContent":"UPGRADE 1 Monthly","referenceCode":"TEST123"}'
```

### 2. Check Logs
```
Logs/Payment/payment-2025-12-01.log
```

### 3. Verify Security
- ✅ Invalid IP → Blocked
- ✅ Invalid signature → Blocked  
- ✅ Old timestamp → Blocked
- ✅ Duplicate referenceCode → Ignored

## Security Best Practices Implemented

✅ **Configuration Management** - Externalized configuration  
✅ **IP Whitelisting** - Restrict webhook sources  
✅ **Signature Validation** - Verify webhook authenticity  
✅ **Timestamp Check** - Prevent replay attacks  
✅ **Idempotency** - Handle duplicate webhooks  
✅ **Comprehensive Logging** - Full audit trail  
✅ **Error Handling** - Graceful error responses  
✅ **No Sensitive Data Leak** - Generic error messages to external callers

## Next Steps (Optional)

- [ ] Setup log rotation/archival (sau 90 ngày)
- [ ] Add email alerts cho failed webhooks
- [ ] Monitor log files với external service (ELK, Splunk, etc.)
- [ ] Add rate limiting để chống DDoS
- [ ] Implement database indexes cho SePayTransactions
- [ ] Add retry mechanism cho failed operations

## Documentation

Xem chi tiết tại: `docs/Payment_SePay_Integration_Guide.md`

---

**Implementation Date**: December 1, 2025  
**Status**: ✅ Complete  
**Security Level**: Production-ready
