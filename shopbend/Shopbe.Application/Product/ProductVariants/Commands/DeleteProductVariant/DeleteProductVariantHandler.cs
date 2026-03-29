using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.ProductVariants.Commands.UpdateProductVariant;

namespace Shopbe.Application.ProductsVariants.Commands.DeleteProductVariant;

public class DeleteProductVariantHandler : IRequestHandler<DeleteProductVariantCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductVariantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}