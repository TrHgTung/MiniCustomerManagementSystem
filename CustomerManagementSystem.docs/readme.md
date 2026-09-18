# Hướng Dẫn Cài Đặt & Vận Hành Dự Án
## Mini Customer Management System (Hệ thống Quản lý Khách hàng cơ bản)

Hệ thống Quản lý Thông tin Khách hàng phân quyền hai cấp: **Super Admin (SA)** và **Manager**, tích hợp trang Đăng ký tư vấn trực tuyến (Public), luồng phê duyệt dữ liệu và xuất báo cáo Excel. Xem thêm [trong tài liệu đặc tả](CustomerManagementSystem.docs/Specification%20Document%20-%20Mini%20Customer%20Management%20System.pdf)

---
### **Tài liệu online và Video demo**: https://tungth-dev.pages.dev/projects/mini-customer-management-system

### **Source Code**: https://github.com/TrHgTung/MiniCustomerManagementSystem

### **Postman File Export**: [Customer Management System API.postman_collection.json](./CustomerManagementSystem.docs/Postman-Export/Customer%20Management%20System%20API.postman_collection.json)
---

## 1. Kiến Trúc & Công Nghệ Sử Dụng

### Backend (`CustomerManagementSystem.core.backend`)
- **Framework:** .NET 9 (ASP.NET Core Web API)
- **Kiến trúc:** Repository - Service Layer
- **Cơ sở dữ liệu:** Microsoft SQL Server
- **ORM:** EF Core 9 (Code-First với Migrations)
- **Xác thực & Phân quyền:** JWT Bearer Token (Access Token & Refresh Token)
- **Thư viện xuất Excel:** ClosedXML
- **SendMail**: System.Net.Mail (smtp.gmail.com)
- **API Documentation:** Swagger doc
- **Địa chỉ mặc định:** `http://localhost:4401` (Swagger: `http://localhost:4401/swagger`)

### Frontend (`CustomerManagementSystem.core.frontend`)
- **Framework:** Blazor WebAssembly (.NET 9)
- **Giao diện:** HTML5, CSS3, Bootstrap 5 (Thiết kế đơn giản, dùng sẵn class responsive)
- **Quản lý phiên:** Custom AuthenticationStateProvider kết hợp Browser LocalStorage
- **Địa chỉ mặc định:** `http://localhost:4402`

---

## 2. Yêu Cầu Môi Trường (Prerequisites)

Trước khi bắt đầu, máy tính cần cài đặt các công cụ sau:

1. **.NET 9.0 SDK**: [Tải tại Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Kiểm tra phiên bản bằng lệnh:
     ```bash
     dotnet --version
     ```
2. **Microsoft SQL Server**:
   - Sử dụng: SQL Server 2025, SSMS 22

3. **IDE**:
   - **Visual Studio 2022** 

4. **Git**: Đã cài đặt trên máy, login git account để clone repository.

---

## 3. Các Bước Cài Đặt & Chạy Dự Án Chi Tiết

### Bước 1: Clone mã nguồn từ Git
Mở Terminal và prompt:

```bash
git clone https://github.com/TrHgTung/MiniCustomerManagementSystem.git
cd MiniCustomerManagementSystem
```
---

### Bước 2: Cấu hình chuỗi kết nối Database (Connection String)

Mở file cấu hình của backend:
📂 `CustomerManagementSystem.core.backend/appsettings.json`

Tìm đến mục `ConnectionStrings:DefaultConnection`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=CustomerManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
}
```
---

### Bước 3: Cập nhật Cơ Sở Dữ Liệu (Run Migrations)

Dự án đã chuẩn bị sẵn các bản Migration trong thư mục `Migrations/`

#### Cách 1: Sử dụng .NET CLI (VS Code / Terminal)
1. Cài đặt công cụ `dotnet-ef` toàn cục (nếu máy chưa từng cài):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
2. Chuyển vào thư mục Backend và chạy lệnh:
   ```bash
   cd CustomerManagementSystem.core.backend
   dotnet ef database update
   ```

#### Cách 2: Sử dụng Package Manager Console (Trong Visual Studio)
1. Mở solution `CustomerManagementSystem.sln` bằng Visual Studio.
2. Mở cửa sổ **Tools** $\rightarrow$ **NuGet Package Manager** $\rightarrow$ **Package Manager Console**.
3. Chọn mục **Default project:** là `CustomerManagementSystem.core.backend`.
4. Gõ lệnh:
   ```powershell
   Update-Database
   ```

---

### Bước 4: Khởi chạy Backend (Web API)

1. Mở terminal tại thư mục backend:
   ```bash
   cd CustomerManagementSystem.core.backend
   dotnet run
   ```
2. Terminal sẽ hiển thị thông báo ứng dụng đã khởi chạy thành công:
   ```text
   Now listening on: http://localhost:4401
   ```
3. Mở trình duyệt và truy cập trang Swagger UI để kiểm tra:
   **http://localhost:4401/swagger**

> **Tự động Seed dữ liệu:** Ngay khi Backend chạy lần đầu tiên, hệ thống sẽ tự động khởi tạo (Seed) tài khoản **Super Admin (SA)** mặc định nếu trong database chưa có.

---

### Bước 5: Khởi chạy Frontend (Blazor WebAssembly)

1. Mở một cửa sổ terminal mới (giữ terminal Backend tiếp tục chạy).
2. Chuyển vào thư mục Frontend và khởi chạy:
   ```bash
   cd CustomerManagementSystem.core.frontend
   dotnet run
   ```
3. Terminal sẽ hiển thị:
   ```text
   Now listening on: http://localhost:4402
   ```
4. Mở trình duyệt và truy cập ứng dụng:
   **http://localhost:4402**

---

## 4. Thông Tin Tài Khoản Đăng Nhập & Phân Quyền

Hệ thống định nghĩa hai cấp phân quyền trong hệ thống quản trị:

| Thuộc tính | Tài khoản Super Admin (SA) | Tài khoản Manager |
|---|---|---|
| **Role Code** | `"2"` | `"1"` |
| **Email đăng nhập** | `admin@tungth.com` | *(Do SA tạo qua API/giao diện)* |
| **Mật khẩu mặc định** | `Test@123` | *(Do SA thiết lập)* |
| **Quyền Thêm KH** | Thêm mới trực tiếp (Kích hoạt ngay) | Thêm mới (Trạng thái: Chờ SA duyệt) |
| **Quyền Sửa KH** | Cập nhật trực tiếp | Cập nhật (Trạng thái: Chờ SA duyệt lại) |
| **Quyền Xóa KH** | **Xóa vĩnh viễn** khỏi cơ sở dữ liệu | **Yêu cầu xóa** (Đánh dấu ngày `DeletedAt`, chờ SA duyệt) |
| **Duyệt các yêu cầu xóa KH** | Có trang quản lý & thực hiện xóa hẳn | Không có quyền |
| **Xuất Excel** | Xuất **toàn bộ** dữ liệu (gồm cả chờ duyệt) | Chỉ xuất các KH **đã kích hoạt** (`IsActive = true`) |
| **Quản lý Manager** | Toàn quyền tạo/khóa/xem Manager | Không có quyền |

#### Sample login:
```json
{
  "orgEmail": "admin@tungth.com",
  "orgPassword": "Test@123"
}
```
---

## 5. Danh Sách Màn Hình & Chức Năng Trên Giao Diện

1. **Trang Tư Vấn Khách Hàng (Trang chủ - Public):**
   - URL: `http://localhost:4402/`
   - Dành cho khách hàng bên ngoài nhập form để gửi thông tin tư vấn (Họ tên, SĐT, Email, Năm sinh, Tỉnh/Thành).
   - Dữ liệu gửi lên sẽ tự động có trạng thái ban đầu là `Chờ duyệt`.

2. **Trang Đăng Nhập Quản Trị:**
   - URL: `http://localhost:4402/admin/login`
   - Đăng nhập bằng Email và Mật khẩu của tài khoản SA hoặc Manager.

3. **Trang Quản Lý Khách Hàng (Admin):**
   - URL: `http://localhost:4402/admin/customers`
   - Hiển thị danh sách khách hàng dưới dạng bảng (STT, Mã KH, Họ tên, Email, SĐT, Nơi ở, Trạng thái, Ngày tạo).
   - Bộ lọc tìm kiếm theo từ khóa (Mã KH, tên, email, sđt), lọc theo Tỉnh/Thành phố, lọc theo Năm sinh.
   - Thêm mới khách hàng (Modal popup).
   - Chỉnh sửa thông tin khách hàng (Modal popup).
   - Thao tác xóa: Nút *Yêu cầu Xóa* (cho Manager) hoặc *Xóa vĩnh viễn* (cho SA).
   - Nút *Duyệt* khách hàng (chỉ dành riêng cho SA khi khách hàng chưa kích hoạt).

4. **Trang Danh Sách Yêu Cầu Xóa KH (Dành riêng cho SA):**
   - URL: `http://localhost:4402/admin/pending-deletion`
   - Hiển thị danh sách các khách hàng đang bị Manager gửi yêu cầu xóa (dựa trên mốc thời gian `DeletedAt`).
   - Thao tác **Khôi phục**: Hủy yêu cầu xóa, reset `DeletedAt = null`, kích hoạt lại khách hàng.
   - Thao tác **Xóa hoàn toàn**: Mở modal cảnh báo và xóa vĩnh viễn dữ liệu khỏi hệ thống.

5. **Trang Xuất Dữ Liệu Excel (Hỗ trợ SA & Manager):**
   - URL: `http://localhost:4402/admin/export`
   - Chọn khoảng ngày `FromDate` và `ToDate` cần xuất dữ liệu.
   - Hỗ trợ các nút chọn nhanh: *Hôm nay*, *7 ngày qua*, *30 ngày qua*, *Tháng này*, *Năm nay*.
   - Nhấn **Xuất Excel** để tải trực tiếp file `.xlsx` về máy.

---

## 6. Cấu Trúc Mã Nguồn (Directory Structure)

```text
CustomerManagementSystem/
├── CustomerManagementSystem.core.backend/     # Backend Web API (.NET 9.0)
│   ├── Configurations/                       # Cấu hình tiền tố route, AutoMapper...
│   ├── Controllers/Admin/                    # API Controllers (Auth, Customers, Export, Administrative)
│   ├── Data/
│   │   ├── Context/                          # ApplicationDbContext (EF Core)
│   │   └── DTO/                              # Data Transfer Objects (Auth, Customer, Manager...)
│   ├── Entities/                             # Entity Models (Customer, OrgMember, RefreshToken)
│   ├── Helpers/                              # UserRoleHelper, Hashing...
│   ├── Migrations/                           # Các bản migration cơ sở dữ liệu
│   ├── Repositories/                         # Repository Pattern (Interface & Implement)
│   ├── Services/                             # Business Logic Layer (Auth, Customer, Export Excel, Email...)
│   ├── appsettings.json                      # File cấu hình ConnectionString, JWT, Port
│   └── Program.cs                            # Cấu hình DI, Middleware, Pipeline & Seed Admin
│
├── CustomerManagementSystem.core.frontend/    # Frontend Web App (Blazor WebAssembly)
│   ├── Auth/                                 # CustomAuthenticationStateProvider
│   ├── Constants/                            # Hằng số tỉnh thành, RoleConstants
│   ├── Helpers/                              # UserRoleHelper (Extension methods kiểm tra quyền SA/Manager)
│   ├── Layout/                               # MainLayout, NavMenu (Sub-menu Admin)
│   ├── Models/                               # DTO nhận/gửi từ API
│   ├── Pages/
│   │   ├── Home.razor                        # Trang form nhập thông tin khách hàng (Public)
│   │   └── Admin/
│   │       ├── Login.razor                   # Màn hình đăng nhập quản trị
│   │       ├── Customers.razor               # Bảng danh sách & Thêm/Sửa/Xóa KH
│   │       ├── PendingDeletion.razor         # Trang duyệt xóa KH dành cho SA
│   │       └── ExportExcel.razor             # Trang lọc và xuất báo cáo Excel
│   ├── Services/                             # HTTP Client Services giao tiếp Backend
│   ├── wwwroot/                              # CSS, index.html (tích hợp script tải file Blob)
│   └── Program.cs                            # Cấu hình DI & Base Address kết nối Backend
│
└── CustomerManagementSystem.docs/            # Tài liệu dự án, sơ đồ use-case, db-diagram
    ├── db-diagram.png                        # Sơ đồ cơ sở dữ liệu
    ├── use-case-diagram.png                  # Sơ đồ Use-Case hệ thống
    ├── Specification Document...pdf          # Đặc tả yêu cầu phần mềm
    └── readme.md                             # Tài liệu hướng dẫn cài đặt & vận hành
```

---

## 7. Xử Lý Sự Cố Thường Gặp (Troubleshooting)

### 1. Lỗi kết nối Database (`Cannot open database "CustomerManagement"` hoặc `Login failed for user`)
- **Nguyên nhân:** Tên instance SQL Server chưa đúng hoặc Service SQL Server chưa được bật.
- **Khắc phục:** 
  - Mở `services.msc` kiểm tra dịch vụ `SQL Server (MSSQLSERVER)` hoặc `SQL Server (SQLEXPRESS)` đang ở trạng thái **Running**.
  - Kiểm tra lại chuỗi `Server=...` trong file `appsettings.json`.

### 2. Lỗi `A connection was successfully established with the server, but then an error occurred during the login process`
- **Nguyên nhân:** Lỗi chứng chỉ SSL bảo mật trên SQL Server.
- **Khắc phục:** Đảm bảo chuỗi kết nối đã có `TrustServerCertificate=True;`.

### 3. Lỗi Frontend không gọi được API (`NetworkError when attempting to fetch resource`)
- **Nguyên nhân:** Backend chưa chạy hoặc port không khớp.
- **Khắc phục:**
  - Đảm bảo Backend đang chạy tại port `4401`.
  - Mở `http://localhost:4401/swagger` xem API có phản hồi không.
  - Kiểm tra cấu hình CORS trong `CustomerManagementSystem.core.backend/Program.cs` đã cho phép `http://localhost:4402`.

### 4. Lỗi khi chạy `dotnet ef database update` (`The term 'dotnet-ef' is not recognized`)
- **Khắc phục:** Chạy lệnh cài đặt công cụ EF Core CLI:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
  Sau đó khởi động lại Terminal.

### 5. Lỗi không gửi được email
- **Khắc phục:** Hãy cấu hình SMTP trong appsettings.json của source backend (CustomerManagementSystem.core.backend/appsettings.json)
```json
    "Smtp": {
      "SenderEmail": "[EMAIL_ADDRESS]",
      "Host": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "[EMAIL_ADDRESS]",
      "Password": "your-gmail-app-password"
    }
```

## 8. Tài liệu liên quan:
- [File Tài liệu đặc tả yêu cầu phần mềm](CustomerManagementSystem.docs/Specification%20Document%20-%20Customer%20Management%20System%20-%20Version%201.0.pdf)
- Tài liệu online và Video demo: https://tungth-dev.pages.dev/projects/mini-customer-management-system
- Source code: https://github.com/TrHgTung/MiniCustomerManagementSystem
- Hoang Tung (TungTH)
