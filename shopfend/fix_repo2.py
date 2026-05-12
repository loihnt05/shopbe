import sys

file_path = '../shopbend/Shopbe.Infrastructure/Repositories/ReviewRepositories/ReviewRepository.cs'

with open(file_path, 'r') as f:
    content = f.read()

old_code = """        var query =
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

new_code = """        var query =
            from x in purchased
            join r in context.Reviews.AsNoTracking().Where(r => r.UserId == userId)
                on new { x.OrderId, x.ProductId } equals new { r.OrderId, r.ProductId } into rJoin
            from r in rJoin.DefaultIfEmpty()
            select new
            {
                x.OrderId,
                x.ProductId,
                x.ProductName,
                ProductImageUrl = context.ProductImages.AsNoTracking()
                    .Where(img => img.ProductId == x.ProductId)
                    .OrderByDescending(img => img.IsPrimary)
                    .ThenBy(img => img.SortOrder)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault(),
                x.PurchasedAt,
                IsReviewed = r != null,
                ReviewId = r != null ? (Guid?)r.Id : null
            };"""

old_code2 = """        if (onlyNotReviewed)
            query = query.Where(x => !x.IsReviewed);

        // Deduplicate if same product appears multiple times in same order (multiple variants/items)
        // Keep deterministic ordering: newest purchases first.
        var result = await query
            .OrderByDescending(x => x.PurchasedAt)
            .ThenBy(x => x.ProductName)
            .ToListAsync(cancellationToken);

        return result
            .GroupBy(x => new { x.OrderId, x.ProductId })
            .Select(g => g.First())
            .ToList();"""

new_code2 = """        if (onlyNotReviewed)
            query = query.Where(x => !x.IsReviewed);

        // Deduplicate if same product appears multiple times in same order (multiple variants/items)
        // Keep deterministic ordering: newest purchases first.
        var result = await query
            .OrderByDescending(x => x.PurchasedAt)
            .ThenBy(x => x.ProductName)
            .ToListAsync(cancellationToken);

        return result
            .Select(x => new ReviewableProductDto(
                x.OrderId,
                x.ProductId,
                x.ProductName,
                x.ProductImageUrl,
                x.PurchasedAt,
                x.IsReviewed,
                x.ReviewId))
            .GroupBy(x => new { x.OrderId, x.ProductId })
            .Select(g => g.First())
            .ToList();"""

if old_code in content and old_code2 in content:
    content = content.replace(old_code, new_code)
    content = content.replace(old_code2, new_code2)
    with open(file_path, 'w') as f:
        f.write(content)
    print("Replaced successfully")
else:
    print("Could not find old_code")
