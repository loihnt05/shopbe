using MediatR;
using Shopbe.Application.Interfaces;

namespace Shopbe.Application.ProductsImages.Commands.DeleteProductImage;

public class DeleteProductImageHandler : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteProductImageHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {

        return true;
    }           
}