# Code Showcase

Trang web tĩnh giúp bạn lưu và chia sẻ các đoạn code Python, C và C++ trên GitHub Pages.

## Cách sử dụng

1. **Chạy backend realtime (.NET 8 + SignalR)**
   - `cd backend/CodeShowcase.Api`
   - `dotnet restore && dotnet run`
   - API mặc định chạy ở `http://localhost:5273`, lưu dữ liệu vào `data/codes.json`.
   - Deploy backend lên dịch vụ hỗ trợ ASP.NET (Azure App Service, Fly.io, Render...) để người khác truy cập realtime.
2. **Cấu hình frontend**
   - Trong `index.html`, cập nhật `window.BACKEND_URL` trỏ tới URL backend bạn vừa deploy.
   - Trang sẽ gọi các endpoint `/api/codes` (GET/POST/DELETE) và kết nối SignalR `/hub/codes`.
3. **Thêm/Xóa code**
   - Form bên trái gửi request POST tới backend để thêm bài (tên, ngôn ngữ, nội dung).
   - Nút **Xóa** gọi DELETE. Mọi người đang mở trang sẽ nhận sự kiện SignalR và thấy thay đổi ngay không cần refresh.
4. **Xuất dữ liệu thủ công (tuỳ chọn)**
   - Nút **Tải codes.json** tạo snapshot hiện tại (lấy trực tiếp từ backend) để bạn lưu trữ/commit.
5. **Host frontend trên GitHub Pages**
   - Settings → Pages → chọn branch `main` và thư mục root.
   - URL mặc định: `https://<username>.github.io/<repo>/`.

> Vì GitHub Pages chỉ là tĩnh, realtime hoạt động nhờ backend ASP.NET đứng riêng xử lý SignalR. Hãy đảm bảo backend public và được bật HTTPS khi triển khai thực tế.

## Tùy biến

- Thêm mã nguồn mẫu trong `data/codes.json`.
- Đổi màu sắc trong `styles.css`.
- Nếu cần realtime thực sự, hãy triển khai backend (Firebase, Supabase, server riêng) rồi cập nhật `app.js` để gọi API đó.

