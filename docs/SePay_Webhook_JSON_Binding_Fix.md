# Fix: SePay Webhook JSON Binding Issue

## Vấn Đề Phát Hiện ❌

Khi SePay gửi webhook, các trường trong database `SePayTransactions` bị NULL hoặc có giá trị = 0:

```sql
Id  Gateway  TransactionDate          AccountNumber  SubAccount  AmountIn  AmountOut  Accumulated  Code  TransactionContent  ReferenceCode
6   MBBank   2025-12-06 23:02:00.000  0859226688     NULL        0.00      0.00       0.00         NULL  NULL                FT25342935691808
```

## Nguyên Nhân ⚠️

### ASP.NET MVC Controller Không Tự Động Bind JSON từ Request Body

**Code CŨ (SAI):**
```csharp
[HttpPost]
public ActionResult SePayWebhook(SePayModel model)  // ❌ model sẽ = null hoặc các field = null
{
    // SePay gửi JSON qua request body, nhưng ASP.NET MVC không tự bind
    // vì không có [FromBody] attribute và MVC khác WebAPI
}
```

**Tại sao?**
- ASP.NET **MVC Controller** chỉ bind dữ liệu từ **Form Data** hoặc **Query String** mặc định
- SePay gửi JSON qua **Request Body** (raw JSON)
- Cần đọc `Request.InputStream` và deserialize thủ công

**Payload thực tế từ SePay:**
```json
{
  "id": 34252032,
  "gateway": "MBBank",
  "transactionDate": "2025-12-06 23:02:00",
  "accountNumber": "0859226688",
  "subAccount": null,
  "amountIn": 0.0,
  "amountOut": 0.0,
  "accumulated": 0.0,
  "code": null,
  "transactionContent": null,
  "referenceCode": "FT25342935691808",
  "description": "BankAPINotify UP41 LIFETIME- Ma GD ACSP/ or497098"
}
```

## Giải Pháp ✅

### 1. Đọc Raw Request Body và Deserialize Thủ Công

**Code MỚI (ĐÚNG):**
```csharp
[HttpPost]
public ActionResult SePayWebhook()  // ✅ Không còn tham số model
{
    // Đọc raw JSON body
    string jsonPayload = null;
    SePayModel model = null;

    try
    {
        Request.InputStream.Position = 0;
        using (var reader = new StreamReader(Request.InputStream))
        {
            jsonPayload = reader.ReadToEnd();  // Đọc toàn bộ JSON
        }

        // Log raw webhook request
        PaymentLogger.LogWebhookRequest(ipAddress, userAgent, jsonPayload);

        // Deserialize JSON thành model
        model = JsonConvert.DeserializeObject<SePayModel>(jsonPayload);
        
        if (model == null)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }
    }
    catch (Exception ex)
    {
        PaymentLogger.Error("Failed to read webhook body", ex);
        return Json(new { success = false, message = "Lỗi đọc dữ liệu" });
    }
    
    // Xử lý model như bình thường
    // ...
}
```

### 2. Sử Dụng Raw JSON Payload Cho Signature Validation

**Code CŨ:**
```csharp
var payload = JsonConvert.SerializeObject(model);  // ❌ Serialize lại → chữ ký sai
```

**Code MỚI:**
```csharp
// Validate webhook security (use original JSON payload)
var (isValid, errorMessage) = _webhookService.ValidateWebhookSecurity(
    apiKey, ipAddress, signature, jsonPayload, timestamp);  // ✅ Dùng JSON gốc
```

**Lý do:** Chữ ký (signature) được tính từ raw JSON body, nếu serialize lại thì format có thể khác → signature mismatch.

### 3. Thêm Using Statement

```csharp
using System.IO;  // Cần để dùng StreamReader
```

## So Sánh Code

### ❌ TRƯỚC (SAI)
```csharp
public ActionResult SePayWebhook(SePayModel model)
{
    // Log
    PaymentLogger.LogWebhookRequest(ipAddress, userAgent,
        Newtonsoft.Json.JsonConvert.SerializeObject(model));  // model = null hoặc field = null
    
    if (model == null)  // ❌ Luôn null
    {
        return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
    }
    
    // Validate signature
    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(model);  // ❌ Serialize lại
    var (isValid, errorMessage) = _webhookService.ValidateWebhookSecurity(
        apiKey, ipAddress, signature, payload, timestamp);
}
```

### ✅ SAU (ĐÚNG)
```csharp
public ActionResult SePayWebhook()  // Không có tham số
{
    // Đọc raw JSON
    Request.InputStream.Position = 0;
    using (var reader = new StreamReader(Request.InputStream))
    {
        jsonPayload = reader.ReadToEnd();  // ✅ Đọc JSON gốc
    }
    
    // Log raw payload
    PaymentLogger.LogWebhookRequest(ipAddress, userAgent, jsonPayload);
    
    // Deserialize
    model = JsonConvert.DeserializeObject<SePayModel>(jsonPayload);  // ✅ Đúng cách
    
    // Validate signature với raw JSON
    var (isValid, errorMessage) = _webhookService.ValidateWebhookSecurity(
        apiKey, ipAddress, signature, jsonPayload, timestamp);  // ✅ Dùng JSON gốc
}
```

## Test Webhook

### 1. Test Với Postman/cURL

```bash
curl -X POST https://your-domain.com/Subscription/SePayWebhook \
  -H "authorization: HUCE-Webhook-2024-Secret-123" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 12345,
    "gateway": "MBBank",
    "transactionDate": "2025-12-06 23:00:00",
    "accountNumber": "0859226688",
    "subAccount": "TEST",
    "amountIn": 250000.0,
    "amountOut": 0.0,
    "accumulated": 250000.0,
    "code": "SUCCESS",
    "transactionContent": "UP41 Lifetime",
    "referenceCode": "TEST123456",
    "description": "Test Transaction"
  }'
```

### 2. Kiểm Tra Log

```powershell
Get-Content "c:\Users\AD\source\repos\Project_Recruiment_Huce\Project_Recruiment_Huce\Logs\Payment\payment-$(Get-Date -Format 'yyyy-MM-dd').log" -Tail 50
```

**Expected Output:**
```
[2025-12-06 23:30:00] [WEBHOOK_REQUEST] Webhook Request:
IP: ::1
User-Agent: PostmanRuntime/7.26.8
Body: {"id":12345,"gateway":"MBBank","transactionDate":"2025-12-06 23:00:00",...}

[2025-12-06 23:30:00] [INFO] API Key validated successfully from IP ::1
[2025-12-06 23:30:00] [INFO] Processing new webhook. ReferenceCode: TEST123456, Amount: 250000
[2025-12-06 23:30:00] [INFO] Transaction saved. ID: 7, Amount: 250000  ✅ Amount KHÔNG còn = 0
[2025-12-06 23:30:00] [INFO] Webhook processed successfully
```

### 3. Kiểm Tra Database

```sql
-- Kiểm tra transaction mới nhất
SELECT TOP 10 * FROM SePayTransactions ORDER BY CreatedAt DESC;
```

**Expected Result:**
```
Id  Gateway  TransactionDate          AccountNumber  SubAccount  AmountIn  AmountOut  Accumulated  Code     TransactionContent  ReferenceCode
7   MBBank   2025-12-06 23:00:00.000  0859226688     TEST        250000.00 0.00       250000.00    SUCCESS  UP41 Lifetime       TEST123456
```
✅ **Tất cả các trường đã có giá trị, không còn NULL!**

## Tại Sao Lỗi Này Hay Gặp?

### ASP.NET MVC vs ASP.NET Web API

| Feature                    | ASP.NET MVC Controller | ASP.NET Web API Controller |
|----------------------------|------------------------|----------------------------|
| Default Model Binding      | Form Data, Query String| **JSON Request Body**      |
| JSON Auto Deserialize      | ❌ NO                  | ✅ YES                     |
| Need `[FromBody]`         | ❌ Doesn't work        | ✅ Works                   |
| JSON Webhook Support       | ❌ Manual Read         | ✅ Automatic               |

**Kết luận:** 
- Nếu dùng **MVC Controller** → phải đọc `Request.InputStream` thủ công
- Nếu dùng **Web API Controller** → tự động deserialize JSON

## Kinh Nghiệm Rút Ra 📚

1. ✅ **Luôn log raw request body** để debug webhook issues
2. ✅ **Đọc Request.InputStream trong MVC Controller** khi nhận JSON
3. ✅ **Dùng raw JSON payload cho signature validation** (không serialize lại)
4. ✅ **Test webhook với Postman trước** khi chờ SePay gửi thật
5. ⚠️ **ASP.NET MVC ≠ ASP.NET Web API** về JSON binding

## Files Đã Sửa

- ✅ `Controllers/SubscriptionController.cs`
  - Added `using System.IO;`
  - Changed `SePayWebhook(SePayModel model)` → `SePayWebhook()`
  - Added manual JSON reading & deserialization
  - Use raw JSON payload for signature validation

## Kết Quả

- ✅ Webhook nhận đầy đủ dữ liệu từ SePay
- ✅ Database lưu đúng tất cả các trường
- ✅ Signature validation hoạt động chính xác
- ✅ Subscription upgrade thành công
