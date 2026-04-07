using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.ProductVariants.Commands.DeleteProductVariant;

public class DeleteProductVariantHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductVariantCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}