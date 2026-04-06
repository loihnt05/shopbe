using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.ProductVariants.Commands.CreateProductVariant;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductVariants.Commands.CreateProductVariant;

public class CreateProductVariantHandler : IRequestHandler<CreateProductVariantCommand, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductVariantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductVariantResponseDto> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}