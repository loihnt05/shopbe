using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Review.Dtos;

namespace Shopbe.Application.Review.Queries.GetMyReviewableProducts;

public sealed record GetMyReviewableProductsQuery(bool OnlyNotReviewed = false) : IRequest<IReadOnlyList<ReviewableProductDto>>;

public sealed class GetMyReviewableProductsQueryHandler(
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<GetMyReviewableProductsQuery, IReadOnlyList<ReviewableProductDto>>
{
    public async Task<IReadOnlyList<ReviewableProductDto>> Handle(GetMyReviewableProductsQuery request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return await unitOfWork.Reviews.ListReviewableProductsForUserAsync(
            user.Id,
            request.OnlyNotReviewed,
            cancellationToken);
    }
}

