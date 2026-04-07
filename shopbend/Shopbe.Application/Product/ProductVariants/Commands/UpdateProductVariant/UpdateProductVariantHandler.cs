using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Commands.UpdateProductVariant;

public class UpdateProductVariantHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductVariantCommand, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductVariantResponseDto> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}