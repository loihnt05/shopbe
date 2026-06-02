# Role-Based Access Control (RBAC) System Plan

## Overview

Complete role-based access control for the ShopBee e-commerce platform with three roles: **Admin**, **Seller**, and **Customer**. Each role has distinct permissions, UI, dashboard, and available actions.

---

## 1. Current State Analysis

| What Exists | What's Missing |
|---|---|
| `UserRole` enum: Admin, Customer, Seller, Staff | `SellerProfile` entity |
| `User.Role` field in DB | Product → Seller relationship |
| Keycloak JWT role mapping in backend | Order → Seller relationship |
| Keycloak auth + UserSyncMiddleware | Frontend role handling in NextAuth |
| Customer pages (profile, orders, cart, etc.) | Admin dashboard (all pages) |
| 17 API controllers | Seller dashboard (all pages) |
| `[Authorize(Roles = "Admin")]` on 2 endpoints | Role-based route protection |

### Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8.0 |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / Web) |
| ORM | Entity Framework Core 8.0 (PostgreSQL) |
| CQRS | MediatR 14.1 |
| Mapping | AutoMapper 15.1 |
| Auth | Keycloak OIDC + JWT Bearer |
| Frontend | Next.js 16 + React 19 + Tailwind CSS 4 |
| Session | NextAuth v4 |

---

## 2. Database Schema

### 2.1 New Entity: SellerProfile

```
SellerProfile
├── Id                    (Guid, PK)
├── UserId                (Guid, FK → User, unique index)
├── ShopName              (string, required, max 200)
├── ShopDescription       (string, nullable, max 2000)
├── ShopLogoUrl           (string, nullable)
├── ShopBannerUrl         (string, nullable)
├── ContactPhone          (string, nullable)
├── ContactEmail          (string, nullable)
├── Address               (string, nullable)
├── City                  (string, nullable)
├── Status                (SellerStatus enum)
├── CommissionRate        (decimal, default 5%)
├── Rating                (decimal, nullable)
├── TotalSales            (int, default 0)
├── TotalRevenue          (decimal, default 0)
├── CreatedAt             (DateTime)
├── UpdatedAt             (DateTime)
└── Navigation: User      (1:1)
```

### 2.2 Modified Entities

**Product — add fields:**
```
+ SellerId          (Guid, FK → User, required)
+ ApprovalStatus    (ApprovalStatus enum: Pending, Approved, Rejected, Hidden)
+ AdminNotes        (string, nullable)
+ Navigation: Seller (User)
```

**OrderItem — add field:**
```
+ SellerId          (Guid, FK → User, denormalized for query efficiency)
```

**User — add navigation:**
```
+ SellerProfile     (optional 1:1)
```

### 2.3 New Enums

```csharp
public enum SellerStatus
{
    Active,
    Suspended,
    Pending
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Hidden
}
```

### 2.4 Existing Enums (unchanged)

```csharp
public enum UserRole { Admin, Customer, Seller, Staff }
public enum UserStatus { Active, Inactive, Banned, PendingVerification }
public enum OrderStatus { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded }
```

### 2.5 Entity Relationship Diagram

```
User (1) ──── (0..1) SellerProfile
  │
  │ (1)
  │
  ├──── (0..*) Product        [via SellerId]
  │
  ├──── (0..*) Order          [via UserId - customer]
  │
  ├──── (0..*) OrderItem      [via SellerId - seller's items in orders]
  │
  ├──── (0..*) UserAddress
  │
  ├──── (0..*) Review
  │
  └──── (0..*) WishlistItem

Product (1) ──── (0..*) ProductVariant
Product (1) ──── (0..*) ProductImage
Product (1) ──── (0..*) OrderItem
Product (*) ──── (1) Category

Order (1) ──── (0..*) OrderItem
Order (*) ──── (1) User [customer]
```

---

## 3. Backend API Structure

### 3.1 Admin APIs

All admin endpoints require `[Authorize(Roles = "Admin")]`.

| Controller | Route | Method | Description |
|---|---|---|---|
| **AdminDashboardController** | `/api/admin/dashboard` | | |
| | `GET /overview` | GET | Platform stats summary |
| **AdminUsersController** | `/api/admin/users` | | |
| | `GET /` | GET | Paginated user list with filters |
| | `GET /{id}` | GET | User detail |
| | `PUT /{id}/status` | PUT | Activate / Deactivate / Ban user |
| | `PUT /{id}/role` | PUT | Change user role |
| | `DELETE /{id}` | DELETE | Soft delete user |
| **AdminSellersController** | `/api/admin/sellers` | | |
| | `GET /` | GET | Paginated seller list |
| | `GET /{id}` | GET | Seller detail |
| | `PUT /{id}/status` | PUT | Approve / Suspend seller |
| | `GET /{id}/stats` | GET | Seller performance stats |
| **AdminProductsController** | `/api/admin/products` | | |
| | `GET /` | GET | All products with status filter |
| | `PUT /{id}/approval` | PUT | Approve / Reject product |
| | `PUT /{id}/visibility` | PUT | Show / Hide product |
| | `DELETE /{id}` | DELETE | Remove product |
| **AdminOrdersController** | `/api/admin/orders` | | |
| | `GET /` | GET | All orders with status filter |
| | `GET /{id}` | GET | Order detail |
| **AdminAnalyticsController** | `/api/admin/analytics` | | |
| | `GET /revenue` | GET | Revenue by period |
| | `GET /sales` | GET | Sales breakdown |
| | `GET /top-products` | GET | Top selling products |
| | `GET /top-sellers` | GET | Top performing sellers |
| **AdminCategoriesController** | `/api/admin/categories` | | |
| | `GET /` | GET | List all categories |
| | `POST /` | POST | Create category |
| | `PUT /{id}` | PUT | Update category |
| | `DELETE /{id}` | DELETE | Delete category |

### 3.2 Seller APIs

All seller endpoints require `[Authorize(Roles = "Seller")]`.

| Controller | Route | Method | Description |
|---|---|---|---|
| **SellerDashboardController** | `/api/seller/dashboard` | | |
| | `GET /overview` | GET | Own stats summary |
| **SellerProductsController** | `/api/seller/products` | | |
| | `GET /` | GET | Own products only |
| | `POST /` | POST | Create product (→ Pending approval) |
| | `PUT /{id}` | PUT | Update own product |
| | `DELETE /{id}` | DELETE | Delete own product |
| **SellerOrdersController** | `/api/seller/orders` | | |
| | `GET /` | GET | Orders containing own products |
| | `GET /{id}` | GET | Order detail (own only) |
| | `PUT /{id}/status` | PUT | Update processing status |
| **SellerAnalyticsController** | `/api/seller/analytics` | | |
| | `GET /revenue` | GET | Own revenue by period |
| | `GET /sales` | GET | Own sales breakdown |
| | `GET /low-stock` | GET | Low stock products |
| **SellerProfileController** | `/api/seller/profile` | | |
| | `GET /` | GET | Get shop profile |
| | `PUT /` | PUT | Update shop profile |

### 3.3 Customer APIs (existing, unchanged)

All existing endpoints remain customer-facing. The `[Authorize]` attribute already protects them.

| Controller | Route | Description |
|---|---|---|
| ProductController | `/api/products` | Browse, search, filter |
| CartController | `/api/cart` | Cart management |
| OrdersController | `/api/orders` | Place orders, view history |
| WishlistController | `/api/wishlist` | Wishlist management |
| ReviewsController | `/api/reviews` | Write reviews |
| UserController | `/api/users` | Profile management |
| UserAddressController | `/api/user-addresses` | Address management |
| RecommendationsController | `/api/recommendations` | Personalized suggestions |

---

## 4. Frontend Structure

### 4.1 Route Structure

```
app/
├── layout.tsx                          # Root layout (customer, unchanged)
├── page.tsx                            # Homepage (unchanged)
│
├── admin/
│   ├── layout.tsx                      # Admin shell: sidebar + top bar
│   ├── components/
│   │   ├── AdminSidebar.tsx            # Sidebar navigation
│   │   └── AdminHeader.tsx             # Top bar with user info
│   ├── overview/
│   │   └── page.tsx                    # Dashboard overview
│   ├── users/
│   │   └── page.tsx                    # User management
│   ├── sellers/
│   │   └── page.tsx                    # Seller management
│   ├── products/
│   │   └── page.tsx                    # Product moderation
│   ├── orders/
│   │   └── page.tsx                    # Order monitoring
│   ├── analytics/
│   │   └── page.tsx                    # Revenue analytics
│   ├── categories/
│   │   └── page.tsx                    # Category management
│   └── settings/
│       └── page.tsx                    # Platform settings
│
├── seller/
│   ├── layout.tsx                      # Seller shell: sidebar + top bar
│   ├── components/
│   │   ├── SellerSidebar.tsx           # Sidebar navigation
│   │   └── SellerHeader.tsx            # Top bar with user info
│   ├── dashboard/
│   │   └── page.tsx                    # Dashboard overview
│   ├── products/
│   │   ├── page.tsx                    # Product list
│   │   ├── new/
│   │   │   └── page.tsx                # Add product form
│   │   └── [id]/
│   │       └── edit/
│   │           └── page.tsx            # Edit product form
│   ├── orders/
│   │   └── page.tsx                    # Order management
│   ├── analytics/
│   │   └── page.tsx                    # Revenue & stats
│   └── profile/
│       └── page.tsx                    # Shop profile editor
│
├── user/
│   └── page.tsx                        # Customer profile (unchanged)
│
├── products/
│   ├── page.tsx                        # Product listing (unchanged)
│   └── [id]/
│       └── page.tsx                    # Product detail (unchanged)
│
├── cart/
│   └── page.tsx                        # Cart (unchanged)
├── checkout/
│   └── page.tsx                        # Checkout (unchanged)
├── purchases/
│   └── page.tsx                        # Purchase history (unchanged)
├── wishlist/
│   └── page.tsx                        # Wishlist (unchanged)
├── recommendations/
│   └── page.tsx                        # Recommendations (unchanged)
└── chat/
    └── page.tsx                        # Chat (unchanged)
```

### 4.2 Admin Dashboard Pages

**Overview Dashboard (`/admin/overview`):**
- Stats cards: Total Users, Total Customers, Total Sellers, Total Products, Pending Products, Total Orders, Pending Orders, Completed Orders, Total Revenue, Monthly Revenue
- Charts: Monthly revenue line chart, Order status pie chart
- Tables: Top-selling products, Top sellers, Recent orders, Recent registered users

**User Management (`/admin/users`):**
- Searchable/filterable table of all users
- Columns: Name, Email, Role, Status, Registered Date, Actions
- Actions: View detail, Change role, Activate/Deactivate/Ban
- Pagination

**Seller Management (`/admin/sellers`):**
- Table of all sellers
- Columns: Shop Name, Owner, Status, Total Sales, Revenue, Rating, Actions
- Actions: View detail, Approve/Suspend, View stats
- Pagination

**Product Management (`/admin/products`):**
- Table of all products with status filter tabs (All, Pending, Approved, Rejected, Hidden)
- Columns: Product Name, Seller, Category, Price, Status, Actions
- Actions: Approve, Reject, Hide, Remove
- Pagination

**Order Monitoring (`/admin/orders`):**
- Table of all orders with status filter
- Columns: Order ID, Customer, Total, Status, Date, Actions
- Actions: View detail
- Pagination

**Revenue Analytics (`/admin/analytics`):**
- Revenue by period (daily, weekly, monthly, yearly)
- Sales by category
- Sales by seller
- Revenue trends chart

**Category Management (`/admin/categories`):**
- Tree view of categories with subcategories
- CRUD operations
- Drag-and-drop reordering

### 4.3 Seller Dashboard Pages

**Dashboard Overview (`/seller/dashboard`):**
- Stats cards: My Products, Pending Orders, Processing Orders, Shipped Orders, Delivered Orders, My Revenue, This Month Revenue
- Low stock alerts
- Charts: Monthly revenue, Sales by product
- Tables: Recent orders, Best-selling products

**Product Management (`/seller/products`):**
- Table of own products only
- Columns: Product Name, Category, Price, Stock, Status (Approved/Pending/Rejected), Actions
- Actions: Edit, Delete, View
- "Add Product" button → `/seller/products/new`

**Add/Edit Product (`/seller/products/new`, `/seller/products/[id]/edit`):**
- Full product form: Name, Description, Category, Price, Discount Price, Images, Variants, Attributes
- Submit → Product created with `ApprovalStatus = Pending`

**Order Management (`/seller/orders`):**
- Table of orders containing seller's products
- Columns: Order ID, Customer, Items (from this seller), Total, Status, Date, Actions
- Actions: View detail, Update processing status
- Status filter

**Revenue & Analytics (`/seller/analytics`):**
- Total revenue, Monthly revenue
- Sales by product
- Revenue by month chart
- Low stock products alert

**Shop Profile (`/seller/profile`):**
- Editable form: Shop Name, Description, Logo, Banner, Contact Phone, Contact Email, Address, City
- Shop preview

### 4.4 Admin Sidebar Navigation

```
┌─────────────────────────┐
│  🏠 Dashboard           │  → /admin/overview
│  👥 Users               │  → /admin/users
│  🏪 Sellers             │  → /admin/sellers
│  📦 Products            │  → /admin/products
│  🛒 Orders              │  → /admin/orders
│  📊 Analytics           │  → /admin/analytics
│  📁 Categories          │  → /admin/categories
│  ⚙️  Settings            │  → /admin/settings
└─────────────────────────┘
```

### 4.5 Seller Sidebar Navigation

```
┌─────────────────────────┐
│  🏠 Dashboard           │  → /seller/dashboard
│  📦 My Products         │  → /seller/products
│  ➕ Add Product         │  → /seller/products/new
│  🛒 Orders              │  → /seller/orders
│  📊 Revenue             │  → /seller/analytics
│  🏪 Shop Profile        │  → /seller/profile
└─────────────────────────┘
```

---

## 5. Role Permission Matrix

| Action | Admin | Seller | Customer |
|---|---|---|---|
| Browse products | ✅ | ✅ | ✅ |
| View product detail | ✅ | ✅ | ✅ |
| Add to cart / wishlist | ✅ | ✅ | ✅ |
| Place order | ✅ | ✅ | ✅ |
| View own orders | ✅ | ✅ | ✅ |
| Write reviews | ✅ | ✅ | ✅ |
| Manage own profile | ✅ | ✅ | ✅ |
| Manage own addresses | ✅ | ✅ | ✅ |
| **Create/edit/delete products** | ✅ (any) | ✅ (own only) | ❌ |
| **View orders with own products** | ✅ (all) | ✅ (own only) | ❌ |
| **Update order processing status** | ✅ (any) | ✅ (own only) | ❌ |
| **View own revenue/stats** | N/A | ✅ | ❌ |
| **Manage shop profile** | N/A | ✅ | ❌ |
| **Manage all users** | ✅ | ❌ | ❌ |
| **Activate/deactivate/ban users** | ✅ | ❌ | ❌ |
| **Approve/reject/hide products** | ✅ | ❌ | ❌ |
| **View all orders** | ✅ | ❌ | ❌ |
| **View platform revenue** | ✅ | ❌ | ❌ |
| **Manage categories** | ✅ | ❌ | ❌ |
| **Manage shipping zones** | ✅ | ❌ | ❌ |
| **View business statistics** | ✅ | ❌ | ❌ |

---

## 6. Authentication & Authorization Flow

### 6.1 Login Flow

```
User clicks Login → Keycloak login page → Keycloak issues JWT with realm_access.roles
→ NextAuth JWT callback decodes roles from JWT
→ Session contains: { user: { name, email }, accessToken, roles: ["Customer"] }
→ middleware.ts checks role → redirects to correct dashboard
```

### 6.2 Role-Based Redirects (middleware.ts)

```
/admin/*  → requires "Admin" role
           → if not Admin: check if Seller → /seller, else → /
/seller/* → requires "Seller" role
           → if not Seller: check if Admin → /admin, else → /
/user/*   → requires any authenticated user
/*        → public (no auth required)
```

### 6.3 Backend Authorization

```csharp
// Admin endpoints
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase { }

// Seller endpoints
[Authorize(Roles = "Seller")]
public class SellerProductsController : ControllerBase { }

// Mixed endpoints (Admin or Seller)
[Authorize(Roles = "Admin,Seller")]
public class SomeController : ControllerBase { }

// Any authenticated user
[Authorize]
public class CartController : ControllerBase { }
```

### 6.4 Seller Ownership Verification

Every seller endpoint must verify the resource belongs to the current seller:

```csharp
[HttpPut("api/seller/products/{id}")]
public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductDto dto)
{
    var keycloakId = _currentUser.KeycloakId;
    var user = await _unitOfWork.Users.GetByKeycloakIdAsync(keycloakId);
    var product = await _unitOfWork.Products.GetByIdAsync(id);

    if (product == null) return NotFound();
    if (product.SellerId != user.Id)
        return Forbid(); // Cannot modify other seller's products

    // ... update logic
}
```

### 6.5 NextAuth Session Augmentation

```typescript
// types/next-auth.d.ts
declare module "next-auth" {
  interface Session {
    accessToken?: string;
    idToken?: string;
    user?: DefaultSession["user"] & {
      roles?: string[];
    };
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    idToken?: string;
    roles?: string[];
  }
}
```

### 6.6 JWT Callback (app/api/auth/[...nextauth]/route.ts)

```typescript
jwt: ({ token, account }) => {
  if (account) {
    token.accessToken = account.access_token;
    // Decode roles from Keycloak JWT realm_access
    const decoded = jwtDecode(account.access_token);
    const realmRoles = decoded?.realm_access?.roles ?? [];
    token.roles = realmRoles;
  }
  return token;
},
session: ({ token, session }) => {
  session.accessToken = token.accessToken;
  session.user.roles = token.roles ?? [];
  return session;
}
```

---

## 7. Implementation Order

### Phase 1: Domain Layer
| Step | File | Description |
|---|---|---|
| 1.1 | `Shopbe.Domain/Enums/SellerStatus.cs` | New enum |
| 1.2 | `Shopbe.Domain/Enums/ApprovalStatus.cs` | New enum |
| 1.3 | `Shopbe.Domain/Entities/Seller/SellerProfile.cs` | New entity |
| 1.4 | `Shopbe.Domain/Entities/Product/Product.cs` | Add SellerId, ApprovalStatus, AdminNotes |
| 1.5 | `Shopbe.Domain/Entities/Order/OrderItem.cs` | Add SellerId |

### Phase 2: Infrastructure Layer
| Step | File | Description |
|---|---|---|
| 2.1 | `Shopbe.Infrastructure/Persistence/Configurations/SellerProfileConfiguration.cs` | EF config |
| 2.2 | `Shopbe.Infrastructure/Persistence/Configurations/ProductConfiguration.cs` | Update FK |
| 2.3 | `Shopbe.Infrastructure/Persistence/Configurations/OrderItemConfiguration.cs` | Update FK |
| 2.4 | `Shopbe.Infrastructure/Persistence/ShopDbContext.cs` | Add DbSets |
| 2.5 | `Shopbe.Infrastructure/Persistence/ShopbeDbSeeder.cs` | Seed admin + default data |
| 2.6 | `dotnet ef migrations add AddSellerAndRBAC` | EF migration |

### Phase 3: Application Layer
| Step | File | Description |
|---|---|---|
| 3.1 | `Shopbe.Application/Common/Interfaces/ICurrentUser.cs` | Add role helpers |
| 3.2 | `Shopbe.Application/Admin/Dtos/` | Admin DTOs (UserDto, SellerDto, DashboardDto, etc.) |
| 3.3 | `Shopbe.Application/Seller/Dtos/` | Seller DTOs (ProductDto, OrderDto, DashboardDto, etc.) |
| 3.4 | `Shopbe.Application/Admin/` | Admin commands and queries |
| 3.5 | `Shopbe.Application/Seller/` | Seller commands and queries |

### Phase 4: Web Layer — Admin Controllers
| Step | File | Description |
|---|---|---|
| 4.1 | `Shopbe.Web/Controllers/AdminDashboardController.cs` | Platform stats |
| 4.2 | `Shopbe.Web/Controllers/AdminUsersController.cs` | User management |
| 4.3 | `Shopbe.Web/Controllers/AdminSellersController.cs` | Seller management |
| 4.4 | `Shopbe.Web/Controllers/AdminProductsController.cs` | Product moderation |
| 4.5 | `Shopbe.Web/Controllers/AdminOrdersController.cs` | Order monitoring |
| 4.6 | `Shopbe.Web/Controllers/AdminAnalyticsController.cs` | Revenue analytics |
| 4.7 | `Shopbe.Web/Controllers/AdminCategoriesController.cs` | Category management |

### Phase 5: Web Layer — Seller Controllers
| Step | File | Description |
|---|---|---|
| 5.1 | `Shopbe.Web/Controllers/SellerDashboardController.cs` | Seller stats |
| 5.2 | `Shopbe.Web/Controllers/SellerProductsController.cs` | Product management |
| 5.3 | `Shopbe.Web/Controllers/SellerOrdersController.cs` | Order management |
| 5.4 | `Shopbe.Web/Controllers/SellerAnalyticsController.cs` | Revenue analytics |
| 5.5 | `Shopbe.Web/Controllers/SellerProfileController.cs` | Shop profile |

### Phase 6: Frontend — Auth & Routing
| Step | File | Description |
|---|---|---|
| 6.1 | `types/next-auth.d.ts` | Augment session with roles |
| 6.2 | `app/api/auth/[...nextauth]/route.ts` | Decode roles from JWT |
| 6.3 | `middleware.ts` | Role-based route protection |
| 6.4 | `lib/shopbeApi.ts` | Add admin/seller API methods |

### Phase 7: Frontend — Admin Dashboard
| Step | File | Description |
|---|---|---|
| 7.1 | `app/admin/layout.tsx` | Admin shell layout |
| 7.2 | `app/admin/components/AdminSidebar.tsx` | Sidebar navigation |
| 7.3 | `app/admin/components/AdminHeader.tsx` | Top bar |
| 7.4 | `app/admin/overview/page.tsx` | Dashboard overview |
| 7.5 | `app/admin/users/page.tsx` | User management |
| 7.6 | `app/admin/sellers/page.tsx` | Seller management |
| 7.7 | `app/admin/products/page.tsx` | Product moderation |
| 7.8 | `app/admin/orders/page.tsx` | Order monitoring |
| 7.9 | `app/admin/analytics/page.tsx` | Revenue analytics |
| 7.10 | `app/admin/categories/page.tsx` | Category management |

### Phase 8: Frontend — Seller Dashboard
| Step | File | Description |
|---|---|---|
| 8.1 | `app/seller/layout.tsx` | Seller shell layout |
| 8.2 | `app/seller/components/SellerSidebar.tsx` | Sidebar navigation |
| 8.3 | `app/seller/components/SellerHeader.tsx` | Top bar |
| 8.4 | `app/seller/dashboard/page.tsx` | Dashboard overview |
| 8.5 | `app/seller/products/page.tsx` | Product list |
| 8.6 | `app/seller/products/new/page.tsx` | Add product |
| 8.7 | `app/seller/products/[id]/edit/page.tsx` | Edit product |
| 8.8 | `app/seller/orders/page.tsx` | Order management |
| 8.9 | `app/seller/analytics/page.tsx` | Revenue & stats |
| 8.10 | `app/seller/profile/page.tsx` | Shop profile |

### Phase 9: Frontend — SiteHeader Updates
| Step | File | Description |
|---|---|---|
| 9.1 | `app/components/SiteHeader.tsx` | Add role-based nav links |
| 9.2 | `app/components/user/UserSidebar.tsx` | Add seller/admin links if applicable |

### Phase 10: Tests
| Step | File | Description |
|---|---|---|
| 10.1 | `tests/Shopbe.Application.Tests/` | Unit tests for admin/seller services |
| 10.2 | `tests/Shopbe.E2E.Tests/AdminFlowTests.cs` | E2E: Admin CRUD operations |
| 10.3 | `tests/Shopbe.E2E.Tests/SellerFlowTests.cs` | E2E: Seller product/order management |
| 10.4 | `tests/Shopbe.E2E.Tests/RoleAccessTests.cs` | E2E: Role-based access control |

---

## 8. File Summary

| Layer | New Files | Modified Files |
|---|---|---|
| Domain | 3 | 2 |
| Infrastructure | 1 | 3 |
| Application | ~14 | 1 |
| Web | 12 | 2 |
| Frontend | ~25 | 5 |
| Tests | ~6 | 2 |
| **Total** | **~61** | **~15** |

---

## 9. Seed Data

```csharp
// Default admin user (configurable via env var ADMIN_KEYCLOAK_ID)
if (!users.Any(u => u.Role == UserRole.Admin))
{
    users.Add(new User
    {
        KeycloakId = adminKeycloakId,
        Email = "admin@shopbee.vn",
        FullName = "System Admin",
        Role = UserRole.Admin,
        Status = UserStatus.Active
    });
}

// Default categories
if (!categories.Any())
{
    categories.AddRange(
        new Category { Name = "Electronics", Slug = "electronics", SortOrder = 1 },
        new Category { Name = "Fashion", Slug = "fashion", SortOrder = 2 },
        new Category { Name = "Home & Living", Slug = "home-living", SortOrder = 3 },
        new Category { Name = "Beauty", Slug = "beauty", SortOrder = 4 },
        new Category { Name = "Sports", Slug = "sports", SortOrder = 5 }
    );
}
```

---

## 10. Key Design Decisions

1. **Seller registration**: Customers can request seller status; Admin approves by changing role to Seller and creating SellerProfile.

2. **Product approval**: New seller products go through `Pending` approval. Admin reviews and approves/rejects.

3. **Commission**: `CommissionRate` on SellerProfile is informational for now. Can be integrated into order calculations later.

4. **OrderItem.SellerId**: Denormalized for query efficiency. When creating an order, the SellerId is copied from Product.SellerId.

5. **Soft delete**: Products use `DeletedAt` for soft delete. Users use `DeletedAt` as well.

6. **Keycloak roles**: Roles are managed in Keycloak and synced to the database via JWT claims. The frontend decodes roles from the JWT token.

7. **Ownership enforcement**: Every seller endpoint verifies `product.SellerId == currentUser.Id` before allowing modifications.

---

## 11. Risk Mitigation

| Risk | Mitigation |
|---|---|
| Existing products have no SellerId | Migration sets default SellerId to admin user for existing products |
| Keycloak role sync issues | Backend validates roles from both JWT and DB |
| Frontend role state stale | middleware.ts refreshes on every navigation |
| Seller accessing other seller's data | Every query filters by SellerId |
| Admin accidentally locked out | Admin role cannot be removed via API |
