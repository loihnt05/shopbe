using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.Products.Commands.DeleteProduct;

public class DeleteProductHandler(IUnitOfWork unitOfWork, ICacheService cache) : IRequestHandler<DeleteProductCommand, bool>
{
    private static readonly string[] CachePrefixesToInvalidate =
    [
        "products:search:",
        "recommendations:"
    ];

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            throw new KeyNotFoundException($"Product with id '{request.Id}' was not found.");

        await unitOfWork.Product.DeleteProductAsync(request.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache-aside invalidation
        await cache.RemoveAsync($"products:id:v2:{request.Id}");
        foreach (var prefix in CachePrefixesToInvalidate)
        {
            await cache.RemoveByPrefixAsync(prefix);
        }

        return true;
    }
}
