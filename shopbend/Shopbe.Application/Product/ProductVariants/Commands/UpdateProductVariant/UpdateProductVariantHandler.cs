using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.ProductVariants.Commands.UpdateProductVariant;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductsVariants.Commands.UpdateProductVariant;

public class UpdateProductVariantHandler : IRequestHandler<UpdateProductVariantCommand, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductVariantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductVariantResponseDto> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}