using AutoMapper;
using Shopbe.Application.Coupon.Dtos;
using CouponEntity = Shopbe.Domain.Entities.Order.Coupon;

namespace Shopbe.Application.Coupon;

public sealed class CouponProfile : Profile
{
    public CouponProfile()
    {
        CreateMap<CouponEntity, CouponResponseDto>();
    }
}

