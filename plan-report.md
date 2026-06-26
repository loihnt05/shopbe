# Báo cáo ngắn đồ án ShopBee

## 1. Tổng quan đồ án

ShopBee là đồ án xây dựng sàn thương mại điện tử full-stack, gồm frontend Next.js, backend ASP.NET Core 8, PostgreSQL, Redis, Keycloak, Stripe, chatbot tư vấn mua sắm và recommendation system. Hệ thống mô phỏng các nghiệp vụ chính của một sàn thương mại điện tử: khách hàng mua hàng, người bán quản lý sản phẩm/đơn hàng, quản trị viên quản lý toàn hệ thống.

Các vai trò chính:

- `Customer`: xem sản phẩm, tìm kiếm, thêm giỏ hàng, đặt hàng, thanh toán, wishlist, review, nhận gợi ý sản phẩm và dùng chatbot.
- `Seller`: quản lý hồ sơ shop, sản phẩm, đơn hàng và xem thống kê.
- `Admin`: quản lý user, seller, sản phẩm, danh mục, đơn hàng, thông báo và analytics.

## 2. Tiến độ và kết quả đạt được

Đồ án đã hoàn thành phần lớn các chức năng cốt lõi:

- Backend .NET 8 theo hướng Clean Architecture.
- Frontend Next.js cho customer, seller và admin.
- PostgreSQL với EF Core migration.
- Keycloak authentication và phân quyền role `Admin`, `Seller`, `Customer`.
- Giỏ hàng, checkout, coupon, đơn hàng, payment, refund.
- Stripe PaymentIntent và webhook xử lý kết quả thanh toán.
- Tracking hành vi người dùng.
- Recommendation system: top-selling, similar products, personalized, frequently bought together, recently viewed.
- Chatbot dùng OpenAI và có tool gọi recommendation thật từ backend.
- Redis cache cho một số truy vấn recommendation.
- Hangfire background job cho dọn behavior hết hạn và retry email.
- Docker Compose cho PostgreSQL, Redis, Keycloak, backend.
- Unit test và E2E test cho các luồng mua hàng, recommendation, admin, seller, profile và phân quyền.

Kết quả đạt được là một hệ thống thương mại điện tử chạy được end-to-end, không chỉ dừng ở CRUD đơn giản mà có các thành phần sát thực tế như xác thực ngoài bằng Keycloak, thanh toán qua Stripe, webhook, cache, background job, chatbot và recommendation.

## 3. Kiến trúc backend .NET 8

Backend được chia thành các project:

- `Shopbe.Domain`: entity, enum, domain model.
- `Shopbe.Application`: DTO, interface, CQRS handler, business rule.
- `Shopbe.Infrastructure`: EF Core, repository, Unit of Work, Redis, Stripe, email, chatbot, recommendation, shipping.
- `Shopbe.Web`: controller, middleware, authentication, authorization, Swagger, CORS, exception handling.
- `tests`: unit test và E2E test.

Điểm nổi bật:

- Controller mỏng, nghiệp vụ được đưa vào handler/service.
- Dùng MediatR theo phong cách CQRS.
- Dùng Unit of Work và transaction cho checkout.
- Dùng EF Core/PostgreSQL để lưu dữ liệu.
- Dùng `UserSyncMiddleware` để đồng bộ user từ Keycloak vào database ứng dụng.
- Dùng global exception middleware và response envelope để chuẩn hóa API.
- Dùng `[Authorize]` và role claim từ Keycloak để bảo vệ API.

## 4. Điểm nổi bật và bài học khi dùng .NET 8

Khi dùng ASP.NET Core 8 cho backend, điểm nổi bật nhất là framework hỗ trợ rất tốt cho việc xây dựng API hiện đại: dependency injection có sẵn, middleware pipeline rõ ràng, cấu hình authentication/authorization chuẩn, tích hợp tốt với Swagger, EF Core, logging và background service. Nhờ đó hệ thống ShopBee có thể tách các phần như controller, service, repository, middleware, payment, cache và authentication thành các lớp riêng, dễ kiểm thử và dễ mở rộng hơn so với cách viết backend tập trung toàn bộ logic vào controller.

Một điểm mạnh khác của .NET 8 là hiệu năng và sự ổn định tốt cho ứng dụng web API. Các luồng quan trọng như checkout, thanh toán, đồng bộ user từ Keycloak, xử lý webhook Stripe và recommendation đều có thể tổ chức theo kiểu strongly typed, async/await và transaction rõ ràng. Khi kết hợp với EF Core, PostgreSQL, Redis và Hangfire, backend không chỉ làm CRUD mà còn xử lý được các bài toán thực tế như cache, job nền, bảo mật API, đồng bộ dữ liệu và tích hợp dịch vụ bên ngoài.

Qua quá trình làm đồ án, em học được cách thiết kế backend theo hướng có cấu trúc hơn: controller chỉ nhận request và trả response, nghiệp vụ chính nằm ở application/service layer, dữ liệu được truy cập qua repository/unit of work, còn các vấn đề hạ tầng như Stripe, Redis, email, Keycloak được đặt ở infrastructure layer. Em cũng hiểu rõ hơn về authentication bằng JWT, phân quyền theo role, transaction khi checkout, webhook bất đồng bộ, caching để giảm tải database và cách viết test cho các luồng nghiệp vụ quan trọng. Đây là những kiến thức có thể áp dụng trực tiếp khi xây dựng các hệ thống web thực tế bằng .NET.

## 5. Xác thực và phân quyền bằng Keycloak

Hệ thống dùng Keycloak làm Identity Provider. Backend không tự lưu mật khẩu, mà xác thực bằng JWT do Keycloak cấp.

Luồng đăng nhập:

1. Người dùng đăng nhập trên frontend.
2. Frontend redirect sang Keycloak.
3. Keycloak xác thực và trả access token về NextAuth.
4. Frontend gọi backend kèm `Authorization: Bearer <access_token>`.
5. Backend validate token theo issuer Keycloak.
6. Backend đọc role từ token và map sang role claim của ASP.NET Core.
7. `UserSyncMiddleware` tạo hoặc cập nhật user app-side theo `sub`, email, name.
8. Frontend middleware chặn route `/admin`, `/seller`; backend tiếp tục chặn API bằng role.

Cách này tách authentication khỏi business logic, giống hướng triển khai thực tế của các hệ thống dùng OAuth2/OpenID Connect.

## 6. Luồng mua hàng và checkout

Luồng khách hàng:

1. Xem danh sách sản phẩm hoặc chi tiết sản phẩm.
2. Hệ thống ghi nhận hành vi xem sản phẩm.
3. Thêm sản phẩm/variant vào giỏ hàng.
4. Áp dụng coupon nếu có.
5. Checkout bằng địa chỉ đã lưu, địa chỉ mặc định hoặc địa chỉ nhập mới.
6. Backend tạo order, tính subtotal, discount, shipping fee, total.
7. Backend lưu order item với snapshot SKU, tên sản phẩm, đơn giá.
8. Backend consume coupon, track hành vi purchase, cập nhật cart.
9. User thanh toán bằng Stripe.
10. Webhook Stripe cập nhật payment/order.
11. User xem đơn hàng trong lịch sử mua hàng.

Điểm quan trọng là checkout chạy trong transaction để tránh lỗi không nhất quán như tạo order nhưng chưa trừ coupon, hoặc trừ cart nhưng order lỗi.

## 7. Chatbot: cách triển khai và cơ sở lý thuyết

Chatbot được triển khai ở backend trong `ChatbotService`. Cơ chế chính là **LLM + tool/function calling**.

Flow xử lý:

1. User đăng nhập và mở chatbot.
2. Frontend tạo hoặc lấy conversation.
3. User gửi tin nhắn.
4. Backend lưu tin nhắn user.
5. `ChatbotService` lấy tối đa 30 tin nhắn gần nhất làm context.
6. Backend gửi context, system prompt và tool definition vào OpenAI model.
7. Tool được khai báo là `get_product_recommendations`.
8. Nếu model thấy user đang hỏi gợi ý sản phẩm, model gọi tool.
9. Backend thực thi tool bằng cách gọi `RecommendationService`.
10. Kết quả sản phẩm thật từ database được gửi lại model.
11. Model format thành câu trả lời tự nhiên cho người mua.
12. Backend lưu câu trả lời assistant vào database.

Cách này không phải cảm tính. Nó dựa trên mô hình **tool-augmented LLM** hoặc **function calling**, trong đó LLM không tự đoán dữ liệu mà gọi chức năng ngoài để lấy dữ liệu thật. Tài liệu OpenAI mô tả flow tool calling gồm: gửi request kèm tools, model trả tool call, ứng dụng thực thi code, gửi output tool lại model, model tạo final response.

Khi trình bày với giảng viên, có thể nói:

- Chatbot của em không chỉ prompt tĩnh.
- Chatbot có khả năng gọi tool backend để lấy recommendation thật.
- Câu trả lời được grounded vào dữ liệu sản phẩm trong database.
- Hướng này đang được dùng rộng rãi trong các AI assistant hiện nay.
- Tuy nhiên chatbot hiện tại chưa phải RAG vector search đầy đủ, vì chưa dùng embedding/vector database để truy xuất tài liệu. Nó là LLM assistant có function calling đến recommendation service.

## 8. Recommendation system: cách triển khai và cơ sở lý thuyết

Recommendation system được triển khai trong `RecommendationService`, kết hợp dữ liệu sản phẩm, đơn hàng, hành vi user và Redis cache.

Các chiến lược:

- `TopSelling`: gợi ý sản phẩm bán chạy dựa trên order item.
- `SimilarProducts`: gợi ý sản phẩm cùng category và gần giá với sản phẩm đang xem.
- `Personalized`: gợi ý cá nhân hóa theo hành vi user.
- `FrequentlyBoughtTogether`: gợi ý sản phẩm thường được mua cùng.
- `RecentlyViewed`: hiển thị sản phẩm user đã xem gần đây.
- `RandomDiscover`: gợi ý khám phá sản phẩm mới, có exclude sản phẩm đã có.

Luồng personalized:

1. Lấy behavior gần nhất của user.
2. Nhóm theo category.
3. Tính điểm theo hành vi:
   - Purchase có trọng số cao nhất.
   - AddToCart có trọng số trung bình.
   - View/các hành vi khác có trọng số thấp hơn.
4. Chọn top category user quan tâm.
5. Lấy sản phẩm thuộc các category này.
6. Loại sản phẩm user đã mua.
7. Ưu tiên sản phẩm có `SoldCount` cao.
8. Nếu chưa đủ dữ liệu thì fallback sang top-selling.

Cơ sở lý thuyết:

- `TopSelling` là popularity-based recommendation, dùng rất phổ biến khi chưa có dữ liệu cá nhân hóa.
- `SimilarProducts` là content-based recommendation, dựa trên đặc trưng sản phẩm như category và price.
- `Personalized` dùng implicit feedback, tức suy luận sở thích từ hành vi như view, add to cart, purchase thay vì yêu cầu user chấm điểm.
- `FrequentlyBoughtTogether` gần với item-to-item collaborative filtering/co-occurrence, tương tự hướng "customers who bought this also bought" của Amazon.

Điểm cần nói rõ:

- Recommendation của em có cơ sở thực tế, không phải cảm tính.
- Hiện tại hệ thống là rule-based/hybrid heuristic recommender, chưa phải mô hình machine learning được train offline.
- Các trọng số như Purchase > AddToCart > View là thiết kế hợp lý trong đồ án, vì hành vi mua hàng thể hiện ý định mạnh hơn xem sản phẩm.
- Nếu phát triển tiếp, có thể đánh giá bằng precision, recall, NDCG hoặc A/B testing, và nâng cấp sang collaborative filtering/vector embedding.

## 9. Tracking hành vi người dùng

Hệ thống có `BehaviorTrackingService` và API tracking. Mỗi event có thể lưu:

- UserId hoặc SessionId.
- ProductId, CategoryId, OrderId.
- BehaviorType.
- Quantity, Value, Currency.
- Source, Device, Referrer, UserAgent, IP.
- Metadata.
- OccurredAt và ExpiresAt.

Dữ liệu này phục vụ:

- Recently viewed.
- Personalized recommendation.
- Phân tích hành vi.
- Dọn dữ liệu cũ bằng background job.

## 10. Redis và background jobs

Redis được dùng thông qua `ICacheService`, không gọi trực tiếp ở application logic. Service hỗ trợ get/set/remove và TTL.

Redis hiện dùng cho các recommendation query tốn chi phí như top-selling, similar products và frequently bought together. Lợi ích là giảm truy vấn lặp lại vào PostgreSQL.

Hangfire được dùng cho job nền:

- Dọn user behavior hết hạn.
- Recovery email pending/retry.
- Queue email notification.

Các tác vụ này không nên chặn request chính, nên đưa sang background job là hợp lý.

## 11. Thanh toán Stripe và webhook

Thanh toán dùng mô hình chuẩn của Stripe: **PaymentIntent + client confirmation + webhook**.

Các thành phần chính:

- Frontend + Stripe.js: nhận `clientSecret` và xác nhận thanh toán.
- Backend `PaymentsController`: tạo payment intent, lưu payment local, xử lý webhook.
- Backend `StripeService`: gọi Stripe SDK, tạo PaymentIntent, lấy PaymentIntent, tạo refund, verify webhook event.
- Stripe external service: xử lý giao dịch thật và gửi webhook về backend.
- Database: lưu `Payment`, `PaymentTransaction`, `Order`, `OrderStatusHistory`, `Refund`.
- `NotificationService`: gửi thông báo thanh toán thành công/thất bại.

Luồng thanh toán:

1. User checkout, backend tạo order trạng thái `Pending`.
2. Frontend gọi API tạo Stripe PaymentIntent.
3. Backend kiểm tra order thuộc user hiện tại.
4. Backend tạo record `Payment` trạng thái `Pending`.
5. `StripeService.CreatePaymentIntentAsync` gọi Stripe tạo PaymentIntent.
6. Backend lưu `StripePaymentIntentId` và transaction pending.
7. Backend trả `clientSecret` cho frontend.
8. Frontend dùng Stripe.js xác nhận thanh toán.
9. Stripe xử lý thanh toán, có thể bao gồm xác thực bổ sung như 3D Secure.
10. Stripe gọi webhook `POST /api/payments/stripe/webhook`.
11. Backend đọc raw body và kiểm tra `Stripe-Signature`.
12. Nếu event là `payment_intent.succeeded`:
    - cập nhật `Payment.Status = Paid`;
    - cập nhật `Order.Status = Confirmed`;
    - thêm `PaymentTransaction`;
    - thêm `OrderStatusHistory`;
    - gửi notification thanh toán thành công.
13. Nếu event là `payment_intent.payment_failed`:
    - cập nhật `Payment.Status = Failed`;
    - thêm transaction failed;
    - gửi notification thanh toán thất bại.

Cơ chế webhook quan trọng vì thanh toán là bất đồng bộ. Frontend không nên là nguồn sự thật duy nhất cho việc đã thanh toán hay chưa. Nguồn đáng tin cậy là event từ Stripe gửi về server. Hệ thống cũng lưu `LastStripeEventId` để tránh xử lý trùng webhook.

Nếu thầy hỏi có bao nhiêu service thực hiện thanh toán, có thể trả lời theo hai cách:

- Theo kiến trúc tổng thể: có 3 chủ thể chính là frontend/Stripe.js, backend payment API và Stripe.
- Theo backend code: có 2 thành phần trực tiếp là `PaymentsController` điều phối nghiệp vụ và `StripeService` giao tiếp với Stripe. Ngoài ra còn có database và `NotificationService` hỗ trợ cập nhật trạng thái, lưu transaction và gửi thông báo.

## 12. Kiểm thử và triển khai

Đồ án đã có unit test và E2E test.

Unit test kiểm tra các handler như coupon, user, wishlist, order email.

E2E test kiểm tra các flow:

- Mua hàng và thanh toán.
- Recommendation.
- Admin.
- Seller.
- Profile.
- Role access.

E2E test dùng Testcontainers để chạy PostgreSQL tạm thời, fake auth thay Keycloak và fake Stripe service để kiểm tra thanh toán ổn định.

Triển khai:

- Docker Compose chạy PostgreSQL, Redis, Keycloak và backend.
- Production có tài liệu triển khai với GCP VM, Nginx reverse proxy, HTTPS bằng Certbot.
- Domain gồm frontend, API và auth server: `shopbee.page`, `api.shopbee.page`, `auth.shopbee.page`.

## 13. Các điểm mạnh khi trình bày

Các điểm nên nhấn mạnh:

- Backend không phải CRUD đơn giản, mà có Clean Architecture, CQRS, transaction, middleware, service abstraction.
- Keycloak tách authentication khỏi backend, bảo mật và sát thực tế hơn.
- Checkout xử lý nhiều business rule: cart, selected items, địa chỉ, coupon, shipping, order history, tracking, notification.
- Thanh toán dùng Stripe PaymentIntent và webhook, không chỉ mô phỏng thanh toán.
- Chatbot dùng function calling để lấy dữ liệu thật từ backend.
- Recommendation dùng nhiều chiến lược có cơ sở: popularity-based, content-based, implicit feedback, item-to-item/co-occurrence.
- Có Redis cache, Hangfire background job và E2E test.

## 14. Hạn chế và hướng phát triển

Một số điểm có thể cải tiến:

- Redis chưa áp dụng cho toàn bộ read path quan trọng.
- Recommendation chưa có mô hình ML train/evaluate chính thức.
- Chatbot chưa dùng RAG/vector database.
- Shipping chưa tích hợp nhà vận chuyển thật.
- Upload file còn dùng local storage.
- Hangfire đang dùng in-memory storage mặc định, production nên chuyển sang storage bền vững hơn.

Hướng phát triển:

- Bổ sung vector search/RAG cho chatbot.
- Nâng recommendation lên collaborative filtering hoặc embedding-based recommender.
- Thêm metrics đánh giá recommendation như precision, recall, NDCG.
- Tích hợp shipping provider thật.
- Chuyển upload sang object storage.
- Thêm monitoring, logging và audit log.

## 15. Kết luận

ShopBee đã triển khai được một sàn thương mại điện tử tương đối hoàn chỉnh với đầy đủ frontend, backend, database, authentication, authorization, payment, webhook, recommendation, chatbot, cache, background job, deployment và testing.

Điểm nổi bật của đồ án là các module nâng cao như Keycloak, Stripe webhook, Redis, Hangfire, chatbot tool calling và recommendation system đều có cơ sở kỹ thuật thực tế, không chỉ làm theo cảm tính. Đây là nền tảng tốt để tiếp tục phát triển thành một hệ thống thương mại điện tử có khả năng mở rộng hơn.
