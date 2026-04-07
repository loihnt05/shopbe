using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Commands.CreateProductVariant;

public class CreateProductVariantHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductVariantCommand, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductVariantResponseDto> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}