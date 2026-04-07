using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.ProductImages.Commands.DeleteProductImage;

public class DeleteProductImageHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {

        return true;
    }           
}