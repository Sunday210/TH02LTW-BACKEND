# Comic Store API

REST API backend viết bằng **ASP.NET Core 8 Web API**, dữ liệu lưu trong **PostgreSQL** qua **Entity Framework Core 8**.

## Tech Stack

- **ASP.NET Core 8** - Web API (controllers)
- **Entity Framework Core 8** - ORM, code-first + migrations
- **Npgsql** - provider kết nối **PostgreSQL**
- **Swagger / Swashbuckle** - tài liệu và test API tại `/swagger`
- **CORS** - cho phép front-end gọi API bằng Axios

## Cấu trúc project

```
ComicStore.sln
ComicStore.Api/
  ComicStore.Api.csproj
  Program.cs                  # Cấu hình EF Core, CORS, Swagger
  appsettings.json            # ConnectionStrings, Cors, cờ Database
  appsettings.Development.json
  Controllers/
    ComicsController.cs       # CRUD truyện (GET/POST/PUT/DELETE)
    UsersController.cs        # CRUD người dùng
    OrdersController.cs       # CRUD đơn hàng
    CartController.cs         # Giỏ hàng
    TagsController.cs         # Thẻ nhân vật
  Models/                     # User, Comic, CharacterTag, ComicCharacter, CartItem, Order, OrderItem
  Dtos/Dtos.cs                # Request/Response + mapping
  Data/
    AppDbContext.cs           # DbSet + Fluent API
    DbInitializer.cs          # Seed dữ liệu mẫu
  Migrations/                 # EF Core migrations
  Properties/launchSettings.json
docs/axios.md                 # Gọi API bằng Axios
```

## API Endpoints

### Comics - `/api/comics`

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/comics?status=Approved&search=iron&createdById=1` | Danh sách (lọc trạng thái, từ khóa, người đăng) |
| GET | `/api/comics/{id}` | Chi tiết một truyện |
| POST | `/api/comics` | Đăng truyện mới (trạng thái `Pending`) |
| PUT | `/api/comics/{id}` | Cập nhật truyện, kể cả duyệt (`status=Approved`) |
| DELETE | `/api/comics/{id}` | Xóa truyện |

### Users - `/api/users`

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/users?role=BUYER` | Danh sách người dùng |
| GET | `/api/users/{id}` | Chi tiết |
| POST | `/api/users` | Tạo người dùng |
| PUT | `/api/users/{id}` | Cập nhật |
| DELETE | `/api/users/{id}` | Xóa |

### Orders - `/api/orders`

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/orders?userId=1&status=PENDING` | Danh sách đơn hàng |
| GET | `/api/orders/{id}` | Chi tiết đơn |
| POST | `/api/orders` | Tạo đơn, server tự tính giá và trừ kho |
| PUT | `/api/orders/{id}` | Cập nhật trạng thái, giao hàng (hủy đơn hoàn kho) |
| DELETE | `/api/orders/{id}` | Xóa đơn, hoàn kho |

### Cart - `/api/cart`

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/cart?userId=1` | Giỏ hàng kèm tổng tiền |
| POST | `/api/cart` | Thêm hoặc cộng dồn vào giỏ |
| PUT | `/api/cart/{id}` | Đổi số lượng |
| DELETE | `/api/cart/{id}` | Xóa một mục |
| DELETE | `/api/cart?userId=1` | Xóa sạch giỏ |

### Tags - `/api/tags`

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/tags?status=APPROVED` | Danh sách thẻ nhân vật |
| GET | `/api/tags/{id}` | Chi tiết |
| POST | `/api/tags` | Tạo thẻ |
| PUT | `/api/tags/{id}` | Cập nhật, duyệt thẻ |
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

Không commit mật khẩu thật, ưu tiên theo thứ tự **user-secrets -> biến môi trường -> appsettings**:

```bash
# a) User secrets (khuyến nghị khi dev), lưu ngoài repo
cd ComicStore.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=comicstore;Username=postgres;Password=YOUR_PASSWORD"

# b) Biến môi trường
export ConnectionStrings__DefaultConnection="Host=localhost;Database=comicstore;Username=postgres;Password=YOUR_PASSWORD"

# c) appsettings.Production.json trên server (bị gitignore)
```

## 2. Cài đặt và cấu hình EF Core

Các gói khai báo trong `ComicStore.Api.csproj`:

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

Nếu `dotnet ef` báo `You must install .NET to run this application` trên Windows, set biến `DOTNET_ROOT` trỏ tới thư mục cài SDK, ví dụ `C:\Users\<user>\.dotnet`.

## 3. Tạo và chạy Migration

```bash
# Tạo migration sau khi sửa Entity/DbContext
dotnet ef migrations add <TenMigration> --project ComicStore.Api --startup-project ComicStore.Api --output-dir Migrations

# Áp dụng migration lên database (tạo bảng)
dotnet ef database update --project ComicStore.Api --startup-project ComicStore.Api

# Xem SQL mà migration sẽ chạy, không cần kết nối database
dotnet ef migrations script --project ComicStore.Api --startup-project ComicStore.Api --output init.sql

# Liệt kê hoặc rollback
dotnet ef migrations list   --project ComicStore.Api --startup-project ComicStore.Api
dotnet ef migrations remove --project ComicStore.Api --startup-project ComicStore.Api
```

Tùy chọn tự chạy migration và seed khi khởi động: đặt `"Database": { "MigrateOnStartup": true }` trong `appsettings.json`.

## 4. Chạy API

```bash
dotnet run --project ComicStore.Api
```

- API: `http://localhost:5099`, port khai báo trong `ComicStore.Api/Properties/launchSettings.json`
- Swagger UI: `http://localhost:5099/swagger`
- Kiểm tra nhanh: `curl http://localhost:5099/api/comics`

Seed dữ liệu mẫu khi `Database:SeedData = true` và migration đã chạy:

| Email | Mật khẩu | Vai trò |
|---|---|---|
| admin@comicstore.vn | admin123 | SystemAdmin |
| publisher@comicstore.vn | publisher123 | Publisher |
| buyer@comicstore.vn | buyer123 | Buyer |

## 5. Gọi API từ front-end bằng Axios

Xem [docs/axios.md](docs/axios.md): ví dụ GET/POST/PUT/DELETE với Axios, cấu hình baseURL,
bắt lỗi và điều kiện CORS phía backend (`Cors:AllowedOrigins` trong `appsettings.json`).