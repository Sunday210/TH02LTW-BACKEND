# Gọi API bằng Axios (Front-end → Back-end)

Project này chỉ chứa backend. File này là tham chiếu để tích hợp Axios từ front-end
(React/Vue/vanilla) sang ASP.NET Core API ở trên.

## 1. Cài đặt

```bash
npm install axios
```

## 2. Tạo API client dùng chung

```js
// api/client.js
import axios from "axios";

const api = axios.create({
  baseURL: "http://localhost:5099/api",   // đổi sang URL backend khi deploy
  headers: { "Content-Type": "application/json" },
  timeout: 10000,
});

// Bắt lỗi tập trung
api.interceptors.response.use(
  (res) => res,
  (err) => {
    const status = err.response?.status;
    const message = err.response?.data?.message ?? err.message;
    console.error(`[${status}] ${message}`);
    return Promise.reject(err);
  }
);

export default api;
```

## 3. Bốn phương thức HTTP với Axios

| HTTP | Axios | Ví dụ |
|---|---|---|
| Đọc dữ liệu | `api.get(url, config)` | `api.get("/comics")` |
| Tạo mới | `api.post(url, data, config)` | `api.post("/comics", body)` |
| Cập nhật | `api.put(url, data, config)` | `api.put("/comics/1", body)` |
| Xóa | `api.delete(url, config)` | `api.delete("/comics/1")` |

### GET - đọc danh sách / chi tiết

```js
import api from "./api/client";

// Danh sách + query string
export const getComics = (params) => api.get("/comics", { params });          // { status: "Approved", search: "iron" }

// Chi tiết 1 bản ghi
export const getComic = (id) => api.get(`/comics/${id}`);

// Ví dụ dùng
const { data } = await getComics({ status: "Approved" });
console.log(data); // mảng ComicResponse
```

### POST - tạo mới

```js
export const createComic = (comic) =>
  api.post("/comics", {
    title: comic.title,
    issue: comic.issue,
    price: comic.price,
    stock: comic.stock,
    publishYear: comic.publishYear,
    description: comic.description,
    imageUrl: comic.imageUrl,
    createdBy: comic.createdById,
    characters: ["Iron Man", "Spider-Man"],
  });

// Dùng
const { data } = await createComic({ title: "Avengers #1", price: 120000, stock: 10 });
console.log(data.id); // id vừa tạo (HTTP 201)
```

### PUT - cập nhật

```js
export const updateComic = (id, comic) =>
  api.put(`/comics/${id}`, {
    ...comic,
    status: "Approved",      // duyệt truyện
  });

// Dùng
await updateComic(1, { title: "Avengers #1 (Revised)", price: 130000, stock: 12, status: "Approved" });
```

### DELETE - xóa

```js
export const deleteComic = (id) => api.delete(`/comics/${id}`);

// Dùng
await deleteComic(1);   // 204 No Content
```

### Các endpoint còn lại

```js
// Users
api.get("/users", { params: { role: "BUYER" } });
api.post("/users", { email: "a@b.vn", name: "A", password: "123456", role: "Buyer" });
api.put("/users/1",  { email: "a@b.vn", name: "A", role: "Buyer", isVerified: true });
api.delete("/users/1");

// Cart
api.get("/cart", { params: { userId: 1 } });
api.post("/cart", { userId: 1, comicId: 2, quantity: 3 });
api.put("/cart/5", { quantity: 2 });
api.delete("/cart/5");
api.delete("/cart", { params: { userId: 1 } });   // xóa sạch giỏ

// Orders
api.get("/orders", { params: { userId: 1, status: "PENDING" } });
api.post("/orders", {
  userId: 1,
  paymentMethod: "COD",
  shippingName: "Nguyễn Văn A",
  shippingPhone: "0900000000",
  shippingAddress: "Hà Nội",
  items: [{ comicId: 1, quantity: 2 }],
});
api.put("/orders/1",  { status: "CONFIRMED" });
api.delete("/orders/1");

// Tags
api.get("/tags", { params: { status: "APPROVED" } });
api.post("/tags",  { name: "Iron Man", wikiUrl: "https://en.wikipedia.org/wiki/Iron_Man" });
api.put("/tags/1", { name: "Iron Man", status: "APPROVED" });
api.delete("/tags/1");
```

### Xử lý lỗi

```js
try {
  const { data } = await createComic({ title: "", price: -1 });
} catch (err) {
  if (!err.response) {
    console.error("Không kết nối được backend:", err.message);
  } else {
    switch (err.response.status) {
      case 400: console.error("Dữ liệu không hợp lệ:", err.response.data); break;
      case 404: console.error("Không tìm thấy:", err.response.data.message); break;
      case 409: console.error("Xung đột:", err.response.data.message); break;
      default:  console.error("Lỗi server:", err.response.status);
    }
  }
}
```

## 4. CORS - điều kiện để Axios gọi được

Backend đã bật chính sách `FrontendPolicy` trong `Program.cs`. Origins được khai báo trong
`appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [ "http://localhost:5173", "http://localhost:3000" ]
}
```

- Chạy front-end ở port khác → thêm origin đó vào mảng trên.
- Nếu thiếu origin, trình duyệt chặn response và Axios báo `CORS error` (không phải lỗi backend).
- Khi deploy, thêm domain thật của front-end vào `AllowedOrigins`.

## 5. Ghi chú khi chạy local

- Chạy backend: `dotnet run --project ComicStore.Api` → API ở `http://localhost:5099` (port khai báo trong `ComicStore.Api/Properties/launchSettings.json`).
- Muốn chạy bằng `https` local: thêm `https://localhost:7183` vào `applicationUrl` và chạy `dotnet dev-certs https` (bỏ `-k` khỏi lệnh curl nếu chưa tin certificate).