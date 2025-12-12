# VietQR.io + SePay Integration - Complete Setup Guide

## 🎯 Giải pháp: VietQR.io API + SePay Webhook

### Tại sao chọn giải pháp này?

| Tính năng | VietQR.io + SePay | VNPay/MoMo | SePay Link Only |
|-----------|-------------------|------------|-----------------|
| **Chi phí** | ✅ Miễn phí/Rẻ | ❌ 1-2% phí | ✅ Miễn phí |
| **QR Quality** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Tự động hóa** | ✅ 100% | ✅ 100% | ✅ 100% |
| **Setup** | ⭐⭐⭐ (Trung bình) | ⭐ (Khó) | ⭐⭐⭐⭐⭐ |
| **Hợp đồng** | ❌ Không cần | ✅ Cần giấy phép DN | ❌ Không cần |
| **Templates** | 4 loại | 1 loại | 1 loại |
| **Phù hợp** | ⭐ **Startup/SME** | Doanh nghiệp lớn | Cá nhân/Test |

---

## 📋 Step-by-Step Setup

### BƯỚC 1: Đăng ký VietQR.io API (5 phút)

1. Truy cập [https://my.vietqr.io/](https://my.vietqr.io/)
2. **Đăng ký tài khoản** (email + password)
3. **Xác thực email**
4. Đăng nhập vào Dashboard
5. Click **"API Keys"** trong menu
6. Copy:
   - `Client ID` (x-client-id)
   - `API Key` (x-api-key)

#### Free Tier Limits:
- ✅ 1000 requests/tháng (miễn phí)
- ✅ Đủ cho 1000 giao dịch/tháng
- ✅ Không cần thẻ tín dụng

#### Paid Plans (optional):
- **Basic**: 50,000 requests/tháng - 200k VNĐ/tháng
- **Pro**: Unlimited - 500k VNĐ/tháng

---

### BƯỚC 2: Cấu hình Web.config

Cập nhật file `Web.config`:

```xml
<!-- Payment Configuration (Shared) -->
<add key="Payment:AccountNumber" value="0812956789" /><!-- TODO: Thay STK của bạn -->
<add key="Payment:AccountName" value="BUI DAI PHU" /><!-- TODO: Thay tên TK -->
<add key="Payment:BankCode" value="MB" /><!-- MB/VCB/TCB/... -->
<add key="Payment:BankBIN" value="970422" /><!-- MB Bank BIN -->

<!-- VietQR.io API Configuration -->
<add key="VietQR:ClientId" value="3be0fbcb-a066-49c6-8a8e-0d5625a43d15" /><!-- TODO: Paste Client ID -->
<add key="VietQR:ApiKey" value="8f8c1925-6ce6-4eef-9df4-6fc3de717fa8" /><!-- TODO: Paste API Key -->
<add key="VietQR:Template" value="print" /><!-- compact, compact2, qr_only, print -->

<!-- SePay Webhook Configuration -->
<add key="SePay:WebhookSecret" value="your-secret-key-change-this" /><!-- TODO: Đổi secret -->
<add key="SePay:AllowedIPs" value="" /><!-- TODO: Add SePay IPs sau khi test -->
```

#### Danh sách Bank BIN codes phổ biến:

| Ngân hàng | BankCode | BIN (acqId) |
|-----------|----------|-------------|
| MBBank | MB | 970422 |
| Vietcombank | VCB | 970436 |
| Techcombank | TCB | 970407 |
| VietinBank | CTG | 970415 |
| BIDV | BIDV | 970418 |
| ACB | ACB | 970416 |

Xem đầy đủ: [https://api.vietqr.io/v2/banks](https://api.vietqr.io/v2/banks)

---

### BƯỚC 3: Đăng ký SePay Webhook

#### Option A: SePay.vn (Khuyên dùng)

1. Truy cập [https://sepay.vn/](https://sepay.vn/)
2. **Đăng ký tài khoản** với STK ngân hàng của bạn
3. Xác thực danh tính (CCCD + Selfie)
4. Vào **Dashboard** → **Webhook Settings**
5. Cấu hình:
   ```
   Webhook URL: https://your-domain.com/Subscription/SePayWebhook
   Webhook Secret: (tự generate, copy vào Web.config)
   ```
6. Lưu **Webhook IPs** và cập nhật vào `SePay:AllowedIPs`

#### Option B: Casso.vn (Alternative)

Tương tự SePay, Casso cũng hỗ trợ webhook cho transaction notification.

---

### BƯỚC 4: Deploy lên Server Public

**Important**: Webhook chỉ hoạt động khi server của bạn public (có domain/IP public).

#### Option 1: Deploy Production
- Azure App Service
- AWS EC2
- VPS (DigitalOcean, Vultr, etc.)

#### Option 2: Development Testing (Ngrok)

```bash
# Install ngrok
choco install ngrok

# Start IIS Express (port 44300)
.\start-iis-express.bat

# Create tunnel
ngrok http https://localhost:44300

# Copy HTTPS URL: https://abc123.ngrok.io
# Update SePay webhook: https://abc123.ngrok.io/Subscription/SePayWebhook
```

---

### BƯỚC 5: Test Payment Flow

#### Test 1: Generate QR Code

1. Đăng nhập với tài khoản Recruiter
2. Vào `/Subscription/Upgrade?planId=Monthly`
3. Kiểm tra:
   - ✅ QR code hiển thị
   - ✅ Số tiền đúng
   - ✅ Nội dung CK hiển thị: `UPGRADE {RecruiterID} Monthly`

#### Test 2: Payment (Real Money - Cẩn thận!)

```
⚠️ WARNING: Test này sẽ chuyển tiền thật!
Dùng số tiền nhỏ (1,000 VND) để test.
```

1. Quét QR bằng app ngân hàng
2. Xác nhận thanh toán
3. Đợi 30-60 giây
4. Kiểm tra:
   - ✅ Log file: `~/Logs/Payment/payment-{date}.log`
   - ✅ Database: `SePayTransactions` table
   - ✅ Subscription activated: `Recruiters.SubscriptionType`

#### Test 3: Mock Webhook (No Real Money)

```bash
# Test webhook endpoint với Postman/curl
curl -X POST https://your-domain.com/Subscription/SePayWebhook \
  -H "Content-Type: application/json" \
  -d '{
    "gateway": "MB",
    "transactionDate": "2025-12-01 14:30:00",
    "accountNumber": "0812956789",
    "amountIn": 25000,
    "transactionContent": "UPGRADE 1 Monthly",
    "referenceCode": "TEST123456"
  }'
```

Expected Response:
```json
{
  "success": true
}
```

---

## 🔍 Troubleshooting

### Issue 1: QR Code không hiển thị

**Nguyên nhân**: VietQR API key chưa config hoặc sai

**Solution**:
1. Kiểm tra `Web.config`:
   ```xml
   <add key="VietQR:ClientId" value="..." />
   <add key="VietQR:ApiKey" value="..." />
   ```
2. Kiểm tra log: `~/Logs/Payment/payment-{date}.log`
3. Hệ thống sẽ tự động fallback sang QR link đơn giản

### Issue 2: Webhook không được gọi

**Nguyên nhân**: 
- Server không public
- URL webhook sai
- SePay chưa cấu hình đúng

**Solution**:
1. Test webhook manually:
   ```bash
   curl -X POST https://your-domain.com/Subscription/SePayWebhook \
     -H "Content-Type: application/json" \
     -d '{"amountIn":1000,"transactionContent":"test"}'
   ```
2. Kiểm tra SePay dashboard → Webhook Logs
3. Kiểm tra firewall/IIS không block POST request

### Issue 3: Subscription không kích hoạt

**Nguyên nhân**:
- Format nội dung CK sai
- Số tiền không đủ
- RecruiterID không tồn tại

**Solution**:
1. Kiểm tra log: `~/Logs/Payment/payment-{date}.log`
2. Kiểm tra database: `SePayTransactions` table
3. Format đúng: `UPGRADE {RecruiterID} {PlanID}`
   - Example: `UPGRADE 1 Monthly`
   - Short: `UP1 Monthly` (VietQR limit)

---

## 📊 Monitoring & Logs

### Log Locations
```
~/Logs/Payment/payment-2025-12-01.log
```

### Key Log Messages

✅ **Success**:
```
[INFO] VietQR API generated successfully for RecruiterID: 123
[INFO] Transaction logged. ID: 456, Amount: 25000
[INFO] Subscription upgraded successfully. RecruiterID: 123
```

⚠️ **Warnings**:
```
[WARNING] Using fallback QR URL for RecruiterID: 123
[WARNING] Blocked request from unauthorized IP: 1.2.3.4
[WARNING] Amount mismatch. Expected: 25000, Received: 20000
```

❌ **Errors**:
```
[ERROR] VietQR API error: 401 - Unauthorized
[ERROR] Webhook processing failed. ReferenceCode: TEST123
```

### Database Monitoring

```sql
-- Check recent transactions
SELECT TOP 10 * 
FROM SePayTransactions 
ORDER BY CreatedAt DESC;

-- Check subscription status
SELECT RecruiterID, SubscriptionType, SubscriptionExpiryDate, FreeJobPostCount
FROM Recruiters
WHERE SubscriptionType != 'Free';

-- Check payment success rate
SELECT 
    COUNT(*) as TotalTransactions,
    COUNT(CASE WHEN TransactionContent LIKE '%UPGRADE%' THEN 1 END) as SubscriptionPayments
FROM SePayTransactions
WHERE CreatedAt >= DATEADD(day, -30, GETDATE());
```

---

## 💰 Cost Estimation

### Free Tier (0 VND/tháng)
- VietQR.io: 1000 QR codes/tháng
- SePay: Webhook miễn phí
- **Total**: 0 VND ✅

### Startup (200-500k VND/tháng)
- VietQR.io Basic: 200k VND
- SePay: Miễn phí
- Server: 200-300k VND (VPS basic)
- **Total**: 400-500k VND/tháng

### Scale (1-2M VND/tháng)
- VietQR.io Pro: 500k VND
- SePay: Miễn phí
- Server: 500k-1.5M VND (production grade)
- **Total**: 1-2M VND/tháng

---

## 🚀 Next Steps

### Phase 1: Basic (✅ Done)
- [x] VietQR.io API integration
- [x] SePay webhook processing
- [x] Auto subscription activation
- [x] Comprehensive logging

### Phase 2: Enhancement
- [ ] Email notification sau thanh toán
- [ ] SMS notification (optional)
- [ ] Admin panel quản lý giao dịch
- [ ] Báo cáo doanh thu

### Phase 3: Advanced
- [ ] Refund processing
- [ ] Invoice generation
- [ ] Multiple payment methods (VNPay, Momo)
- [ ] Recurring subscription

---

## 📞 Support

### VietQR.io
- Website: https://www.vietqr.io/
- Docs: https://www.vietqr.io/intro
- Support: support@vietqr.io

### SePay
- Website: https://sepay.vn/
- Support: support@sepay.vn

### System Support
- Email: support@example.com
- Phone: 0812956789

---

## 📝 Changelog

### v2.0.0 (2025-12-01)
- ✅ Integrated VietQR.io API
- ✅ Support both full & short content format
- ✅ Fallback mechanism when API unavailable
- ✅ Enhanced UI with better QR display
- ✅ Copy transfer content button
- ✅ Auto payment status check

### v1.0.0 (Previous)
- SePay QR link only
- Basic webhook processing

---

**Status**: ✅ Production Ready  
**Last Updated**: December 1, 2025  
**Recommended For**: Startups, SMEs, MVP Projects
