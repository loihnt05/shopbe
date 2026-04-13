using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Queries.GetAllCoupons;

public class GetAllCouponsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllCouponsQuery, IEnumerable<CouponResponseDto>>
{
    public async Task<IEnumerable<CouponResponseDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var coupons = await unitOfWork.Coupons.GetAllAsync(cancellationToken);

        var query = coupons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Filter.Code))
        {
            var code = request.Filter.Code.Trim();
            query = query.Where(c => c.Code.Contains(code, StringComparison.OrdinalIgnoreCase));
        }

        if (request.Filter.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.Filter.IsActive.Value);
        }

        if (request.Filter.IsExpired.HasValue)
        {
            var now = DateTime.UtcNow;
            query = request.Filter.IsExpired.Value
                ? query.Where(c => c.ExpiredAt <= now)
                : query.Where(c => c.ExpiredAt > now);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        if (request.Filter.Skip is > 0)
        {
            query = query.Skip(request.Filter.Skip.Value);
        }

        if (request.Filter.Take is > 0)
        {
            query = query.Take(request.Filter.Take.Value);
        }

        return query.Select(c => mapper.Map<CouponResponseDto>(c));
    }
}


