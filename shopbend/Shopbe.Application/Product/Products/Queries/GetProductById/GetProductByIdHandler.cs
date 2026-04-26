using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetProductById;

public class GetProductByIdHandler(IUnitOfWork unitOfWork, ICacheService cache)
    : IRequestHandler<GetProductByIdQuery, ProductResponseDto?>
{
    public async Task<ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:id:{request.Id}";

        // 1) Try cache
        var cached = await cache.GetAsync<ProductResponseDto>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        // 2) Load from DB
        var product = await unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            return null;

        var dto = ProductDtoMapper.ToResponse(product);

        // 3) Store to cache
        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

        return dto;
    }
}
