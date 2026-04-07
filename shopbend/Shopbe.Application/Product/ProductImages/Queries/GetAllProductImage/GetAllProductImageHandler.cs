using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Queries.GetAllProductImage;

public class GetAllProductImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductImageQuery, IEnumerable<ProductImageResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<ProductImageResponseDto>> Handle(GetAllProductImageQuery request, CancellationToken cancellationToken)
    {
        var images = await _unitOfWork.ProductImage.GetAllProductImagesAsync();
        var query = images.AsQueryable();

        if (request.Filter.ProductId is not null)
        {
            query = query.Where(i => i.ProductId == request.Filter.ProductId.Value);
        }

        query = query
            .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize);

        return query
            .Select(i => new ProductImageResponseDto(i.Id, i.ImageUrl, i.IsPrimary))
            .ToList();
    }
}