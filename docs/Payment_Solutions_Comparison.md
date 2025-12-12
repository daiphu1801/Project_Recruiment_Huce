# So sánh các giải pháp thanh toán cho Startup/SME

## 🎯 Tổng quan

Bảng so sánh chi tiết các giải pháp thanh toán phổ biến tại Việt Nam cho dự án Recruitment Platform.

---

## 📊 Comparison Matrix

| Tiêu chí | **VietQR + SePay** ⭐ | VNPay Gateway | Momo Gateway | Casso.vn | PayOS |
|----------|---------------------|---------------|--------------|----------|--------|
| **Setup Time** | 30 phút | 2-4 tuần | 2-4 tuần | 15 phút | 30 phút |
| **Chi phí hàng tháng** | 0-500k | 0 | 0 | 0-300k | 0-200k |
| **Phí giao dịch** | 0% | 1-2% | 1.5-2% | 0% | 0.5-1% |
| **Yêu cầu pháp lý** | ❌ Không | ✅ GPKD | ✅ GPKD | ❌ Không | ⚠️ Tùy loại |
| **QR Code quality** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Tự động hóa** | 100% | 100% | 100% | 100% | 100% |
| **Hỗ trợ ngân hàng** | Tất cả | Tất cả | Tất cả | Tất cả | Tất cả |
| **API Documentation** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Webhook Security** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Refund Support** | Manual | Auto | Auto | Manual | Auto |
| **Invoice/Receipt** | Manual | Auto | Auto | Manual | Auto |
| **Phù hợp** | ⭐ MVP/Startup | Enterprise | Enterprise | Startup | Startup/SME |

---

## 💰 Chi phí Chi tiết

### 1. VietQR.io + SePay (Recommended ⭐)

**Setup Cost**: 0 VND

**Monthly Cost**:
- VietQR.io Free: 1000 QR/tháng = **0 VND** ✅
- VietQR.io Basic: 50k QR/tháng = **200k VND**
- VietQR.io Pro: Unlimited = **500k VND**
- SePay: **0 VND** (webhook miễn phí)
- Casso (alternative): **300k VND**/tháng

**Transaction Fee**: **0%** ✅

**Total Monthly** (Free tier): **0 VND**  
**Total Monthly** (Paid): **200-800k VND**

**Phù hợp**:
- ✅ MVP testing
- ✅ Startup < 1000 giao dịch/tháng
- ✅ Không có GPKD
- ✅ Cần QR đẹp, chuyên nghiệp

---

### 2. VNPay Gateway

**Setup Cost**: 
- Phí tích hợp: 5-10 triệu VND
- Deposit: 20-50 triệu VND

**Monthly Cost**:
- Phí duy trì: 0 VND
- Transaction fee: **1-2%** mỗi giao dịch

**Monthly Example** (1000 giao dịch x 50k):
- Doanh thu: 50 triệu
- Phí VNPay (1.5%): **750k VND**

**Yêu cầu**:
- ✅ GPKD (Giấy phép kinh doanh)
- ✅ Website có https
- ✅ Tài liệu pháp lý đầy đủ
- ⏱️ Thời gian duyệt: 2-4 tuần

**Phù hợp**:
- ❌ Startup giai đoạn đầu
- ✅ SME có GPKD
- ✅ Doanh thu > 100 triệu/tháng
- ✅ Cần invoice tự động

---

### 3. Momo Gateway

Tương tự VNPay:

**Setup Cost**: 5-10 triệu VND  
**Transaction Fee**: 1.5-2%  
**Yêu cầu**: GPKD, 2-4 tuần duyệt

**Ưu điểm**:
- Phổ biến với Gen Z
- UI/UX tốt
- Nhiều promotion

**Nhược điểm**:
- Phí cao hơn VNPay
- Yêu cầu pháp lý nghiêm ngặt

---

### 4. Casso.vn

**Setup Cost**: 0 VND

**Monthly Cost**:
- Basic: **300k VND/tháng**
- Pro: **600k VND/tháng**

**Transaction Fee**: **0%**

**Features**:
- Webhook tự động
- API banking
- Multi-bank support
- Reconciliation

**Phù hợp**:
- ✅ Startup
- ✅ Không cần GPKD
- ✅ Cần bank reconciliation
- ❌ Đắt hơn VietQR (nếu dùng API)

---

### 5. PayOS (PayME/PayOS.vn)

**Setup Cost**: 0-2 triệu VND

**Monthly Cost**:
- Free tier: 0 VND (limit giao dịch)
- Paid: 200-500k VND/tháng

**Transaction Fee**: **0.5-1%**

**Features**:
- Payment gateway modern
- QR Code + Link payment
- Good documentation
- Fast integration

**Yêu cầu**:
- Tùy gói: Không GPKD (Basic) hoặc có GPKD (Enterprise)

**Phù hợp**:
- ✅ Startup
- ✅ SME
- ✅ Cần gateway đầy đủ

---

## 🎯 Recommendation by Use Case

### Case 1: MVP/Testing (Budget < 1M VND/tháng)
**→ VietQR.io + SePay** ⭐

**Why?**:
- 0 VND setup cost
- 0% transaction fee
- No legal requirements
- Quick setup (30 minutes)
- Professional QR codes

**Implementation**: Đã làm xong ✅

---

### Case 2: Startup (Doanh thu 50-200M/tháng)
**→ VietQR.io + SePay hoặc PayOS**

**Option A**: VietQR + SePay
- Cost: 500k-1M/tháng
- Pro: Rẻ nhất
- Con: Manual refund/invoice

**Option B**: PayOS
- Cost: 500k + 0.5% transaction fee
- Pro: Gateway đầy đủ, auto refund
- Con: Đắt hơn một chút

---

### Case 3: SME có GPKD (Doanh thu > 200M/tháng)
**→ VNPay hoặc Momo Gateway**

**Why?**:
- Professional payment gateway
- Auto invoice/receipt
- Refund support
- Customer trust (brand name)
- Scale tốt

**Trade-off**:
- Phí 1-2% chấp nhận được với doanh thu lớn
- Cần GPKD và legal docs

---

### Case 4: E-commerce/Marketplace
**→ VNPay + Momo + VietQR (Multi-gateway)**

**Why?**:
- User choice
- Optimize conversion rate
- Reduce single-point failure

---

## 🔄 Migration Path

### Phase 1: MVP (0-6 months)
```
VietQR.io + SePay (Free tier)
→ 0 VND/tháng
→ Test market fit
```

### Phase 2: Growth (6-12 months)
```
VietQR.io + SePay (Paid tier) hoặc PayOS
→ 500k-1M/tháng
→ Scale to 1000+ users
```

### Phase 3: Scale (12+ months)
```
Add VNPay/Momo Gateway
→ Multi-payment support
→ Professional operation
```

---

## ✅ Decision Matrix

### Chọn VietQR + SePay nếu:
- [ ] Startup/MVP giai đoạn đầu
- [ ] Budget < 1M/tháng
- [ ] Chưa có GPKD
- [ ] Giao dịch < 1000/tháng
- [ ] Cần setup nhanh
- [ ] Chấp nhận manual refund/invoice

### Chọn PayOS nếu:
- [ ] Startup có budget 1-2M/tháng
- [ ] Cần payment gateway đầy đủ
- [ ] Cần auto refund/invoice
- [ ] Giao dịch 1000-5000/tháng

### Chọn VNPay/Momo nếu:
- [ ] Có GPKD
- [ ] Doanh thu > 200M/tháng
- [ ] Transaction fee 1-2% chấp nhận được
- [ ] Cần brand trust
- [ ] Scale long-term

---

## 📈 ROI Analysis

### Scenario: 1000 giao dịch/tháng x 50k = 50M doanh thu

| Solution | Monthly Cost | Transaction Fee | Total Cost | ROI |
|----------|-------------|-----------------|------------|-----|
| VietQR + SePay | 500k | 0 | **500k** | **99%** ⭐ |
| PayOS | 500k | 250k (0.5%) | **750k** | **98.5%** |
| VNPay | 0 | 750k (1.5%) | **750k** | **98.5%** |
| Momo | 0 | 1M (2%) | **1M** | **98%** |

**Winner**: VietQR + SePay (500k = 1% của doanh thu)

---

## 🚀 Final Recommendation

### For Your Project (Recruitment Platform):

**Phase 1 (Current)**: ✅ **VietQR.io + SePay**
- Implementation: Done ✅
- Cost: 0-500k/tháng
- Perfect for MVP

**Phase 2 (6 months later)**: Consider adding **PayOS**
- When: Doanh thu > 100M/tháng
- Why: Better UX, auto features

**Phase 3 (12 months later)**: Add **VNPay/Momo**
- When: Doanh thu > 500M/tháng
- Why: Multi-gateway, enterprise ready

---

## 📞 Contacts

### VietQR.io
- Website: https://www.vietqr.io/
- Register: https://my.vietqr.io/
- Support: support@vietqr.io

### SePay
- Website: https://sepay.vn/
- Support: support@sepay.vn

### PayOS
- Website: https://payos.vn/
- Docs: https://payos.vn/docs

### VNPay
- Website: https://vnpay.vn/
- Business: 1900 5555 77

### Momo
- Website: https://business.momo.vn/
- Hotline: 1900 54 54 41

---

**Conclusion**: VietQR.io + SePay là lựa chọn tối ưu nhất cho project của bạn ở giai đoạn hiện tại! ✅

**Status**: Recommended & Implemented  
**Last Updated**: December 1, 2025
