import sys

file_path = '../shopbend/Shopbe.Infrastructure/Repositories/ReviewRepositories/ReviewRepository.cs'

with open(file_path, 'r') as f:
    content = f.read()

old_code = """        // Image (best effort: primary image else lowest sort order)
        var productImage =
            from img in context.ProductImages.AsNoTracking()
            group img by img.ProductId
            into g
            select new
            {
                ProductId = g.Key,
                ImageUrl = g
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.SortOrder)
                    .Select(x => x.ImageUrl)
                    .FirstOrDefault()
            };

        var query =
            from x in purchased
            join img in productImage on x.ProductId equals img.ProductId into imgJoin
            from img in imgJoin.DefaultIfEmpty()
            join r in context.Reviews.AsNoTracking().Where(r => r.UserId == userId)
                on new { x.OrderId, x.ProductId } equals new { r.OrderId, r.ProductId } into rJoin
            from r in rJoin.DefaultIfEmpty()
            select new ReviewableProductDto(
                x.OrderId,
                x.ProductId,
                x.ProductName,
                img.ImageUrl,
                x.PurchasedAt,
                r != null,
                r != null ? r.Id : null);"""

new_code = """        var query =
            from x in purchased
            join r in context.Reviews.AsNoTracking().Where(r => r.UserId == userId)
                on new { x.OrderId, x.ProductId } equals new { r.OrderId, r.ProductId } into rJoin
            from r in rJoin.DefaultIfEmpty()
            select new ReviewableProductDto(
                x.OrderId,
                x.ProductId,
                x.ProductName,
                context.ProductImages.AsNoTracking()
                    .Where(img => img.ProductId == x.ProductId)
                    .OrderByDescending(img => img.IsPrimary)
                    .ThenBy(img => img.SortOrder)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault(),
                x.PurchasedAt,
                r != null,
                r != null ? r.Id : null);"""

if old_code in content:
    with open(file_path, 'w') as f:
        f.write(content.replace(old_code, new_code))
    print("Replaced successfully")
else:
    print("Could not find old_code")
