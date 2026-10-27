# PANEL. — Comic Store API (ASP.NET Core)

Đồ án môn **Lập trình Web** — Học viện Công nghệ Bưu chính Viễn thông (PTIT) cơ sở TP.HCM

Backend REST API viết bằng **ASP.NET Core 8 Web API** + **Entity Framework Core** (PostgreSQL).
Phần frontend (React/Vite) và backend Express cũ đã được loại bỏ khỏi repo — project chỉ còn backend.

## Thành viên nhóm

| Họ tên | MSSV |
|---|---|
| Nguyễn Quang Chí | N23DCAT009 |
| Tô Long Đức | N23DCAT013 |
| Phạm Thái Dương | N23DCAT016 |

## Tech Stack

- **ASP.NET Core 8** — Web API (controllers)
- **Entity Framework Core 8** — ORM, code-first + migrations
- **Npgsql** — provider kết nối **PostgreSQL**
- **Swagger / Swashbuckle** — tài liệu & test API tại `/swagger`
- **CORS** — cho phép front-end gọi API bằng Axios

## Cấu trúc dự án

```
laptrinhweb-comicstore/
├── ComicStore.sln                    # Solution
├── ComicStore.Api/
│   ├── Program.cs                    # Cấu hình DI, EF Core, CORS, Swagger
│   ├── appsettings.json              # ConnectionStrings, Cors, cờ Database
│   ├── appsettings.Development.json
│   ├── Controllers/
│   │   ├── ComicsController.cs       # CRUD truyện  (GET/POST/PUT/DELETE)
│   │   ├── UsersController.cs        # CRUD người dùng
│   │   ├── OrdersController.cs       # CRUD đơn hàng
│   │   ├── CartController.cs         # Giỏ hàng
│   │   └── TagsController.cs         # Thẻ nhân vật Marvel
│   ├── Models/                       # Entity (User, Comic, Order, CartItem, ...)
│   ├── Dtos/Dtos.cs                  # Request/Response contracts + mapping
│   ├── Data/
│   │   ├── AppDbContext.cs           # DbSet + cấu hình Fluent API
│   │   └── DbInitializer.cs          # Seed dữ liệu mẫu
│   └── Migrations/                   # EF Core migrations (code-first)
└── docs/axios.md                     # Hướng dẫn gọi API bằng Axios
```

## API Endpoints

### Comics — `/api/comics`
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/comics?status=Approved&search=iron&createdById=1` | Danh sách (lọc theo trạng thái/từ khóa/người đăng) |
| GET | `/api/comics/{id}` | Chi tiết một truyện |
| POST | `/api/comics` | Đăng truyện mới (trạng thái `Pending`) |
| PUT | `/api/comics/{id}` | Cập nhật truyện (kể cả duyệt: `status=Approved`) |
| DELETE | `/api/comics/{id}` | Xóa truyện |

### Users — `/api/users`
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/users?role=BUYER` | Danh sách người dùng |
| GET | `/api/users/{id}` | Chi tiết |
| POST | `/api/users` | Tạo người dùng |
| PUT | `/api/users/{id}` | Cập nhật |
| DELETE | `/api/users/{id}` | Xóa |

### Orders — `/api/orders`
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/orders?userId=1&status=PENDING` | Danh sách đơn hàng |
| GET | `/api/orders/{id}` | Chi tiết đơn |
| POST | `/api/orders` | Tạo đơn (server tự tính giá + trừ kho) |
| PUT | `/api/orders/{id}` | Cập nhật trạng thái / giao hàng (hủy đơn hoàn kho) |
| DELETE | `/api/orders/{id}` | Xóa đơn (hoàn kho) |

### Cart — `/api/cart`
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/cart?userId=1` | Giỏ hàng + tổng tiền |
| POST | `/api/cart` | Thêm/cộng dồn vào giỏ |
| PUT | `/api/cart/{id}` | Đổi số lượng |
| DELETE | `/api/cart/{id}` | Xóa 1 mục |
| DELETE | `/api/cart?userId=1` | Xóa sạch giỏ |

### Tags — `/api/tags`
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/tags?status=APPROVED` | Danh sách thẻ nhân vật |
| GET | `/api/tags/{id}` | Chi tiết |
| POST | `/api/tags` | Tạo thẻ |
| PUT | `/api/tags/{id}` | Cập nhật / duyệt thẻ |
| DELETE | `/api/tags/{id}` | Xóa thẻ |

## 1. Cấu hình kết nối Database

Connection string nằm trong `ComicStore.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=comicstore;Username=postgres;Password=YOUR_PASSWORD"
}
```

Đọc trong `Program.cs`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Không commit mật khẩu thật** — dùng 1 trong 3 cách sau (EF/ASP.NET đọc theo thứ tự ưu tiên):

```bash
# a) User secrets (khuyến nghị khi dev) — lưu ngoài repo
cd ComicStore.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=comicstore;Username=postgres;Password=YOUR_PASSWORD"

# b) Biến môi trường (Linux/macOS/Git Bash)
export ConnectionStrings__DefaultConnection="Host=localhost;Database=comicstore;Username=postgres;Password=YOUR_PASSWORD"

# c) appsettings.Production.json trên server (bị gitignore)
```

## 2. Cài đặt & cấu hình EF Core

Các gói đã khai báo trong `ComicStore.Api.csproj`:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
```

Cài công cụ line EF (một lần, global):

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

> Nếu `dotnet ef` báo *"You must install .NET to run this application"* trên Windows,
> set biến `DOTNET_ROOT` trỏ tới thư mục cài SDK, ví dụ `C:\Users\<user>\.dotnet`.

## 3. Tạo & chạy Migration

```bash
# Tạo migration sau khi sửa Entity/DbContext
dotnet ef migrations add <TenMigration> --project ComicStore.Api --startup-project ComicStore.Api --output-dir Migrations

# Áp dụng migration lên database (tạo bảng)
dotnet ef database update --project ComicStore.Api --startup-project ComicStore.Api

# Xem SQL mà migration sẽ chạy (không cần kết nối DB)
dotnet ef migrations script --project ComicStore.Api --startup-project ComicStore.Api --output init.sql

# Lọc lịch sử / rollback
dotnet ef migrations list   --project ComicStore.Api --startup-project ComicStore.Api
dotnet ef migrations remove --project ComicStore.Api --startup-project ComicStore.Api
```

Bật tự chạy migration + seed khi khởi động (tùy chọn): đặt `"Database": { "MigrateOnStartup": true }` trong `appsettings.json`.

## 4. Chạy API

```bash
dotnet run --project ComicStore.Api
```

- API: **http://localhost:5099** (port khai báo trong `ComicStore.Api/Properties/launchSettings.json`)
- Swagger UI: **http://localhost:5099/swagger**
- Health check nhanh: `curl http://localhost:5099/api/comics`

Seed dữ liệu mẫu (khi `Database:SeedData = true` và đã chạy migration):

| Vai trò | Email | Mật khẩu |
|---|---|---|
| System Admin | admin@comicstore.vn | admin123 |
| Publisher | publisher@comicstore.vn | publisher123 |
| Buyer | buyer@comicstore.vn | buyer123 |

## 5. Gọi API từ front-end bằng Axios

Xem [docs/axios.md](docs/axios.md) — ví dụ GET/POST/PUT/DELETE với Axios, cách cấu hình
baseURL, interceptor lỗi, và CORS phía backend (`Cors:AllowedOrigins` trong `appsettings.json`).

---

(c) 2026 PANEL. Comic Store — Đồ án Lập trình Web, PTIT HCM