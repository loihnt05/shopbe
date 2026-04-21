using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Review.Dtos;
using Shopbe.Domain.Entities.Review;

namespace Shopbe.Application.Review.Commands.CreateReview;

public sealed record CreateReviewCommand(CreateReviewRequestDto Request) : IRequest<ReviewDto>;

public sealed class CreateReviewCommandHandler(
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        var dto = request.Request;
        if (dto.Rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating must be between 1 and 5.");


        // Prevent duplicates (one review per user per product per order)
        var existing = await unitOfWork.Reviews.GetByOrderAndProductForUserAsync(dto.OrderId, dto.ProductId, user.Id,
            cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Review already exists for this product in this order.");

        var review = new Shopbe.Domain.Entities.Review.Review
        {
            UserId = user.Id,
            ProductId = dto.ProductId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsVisible = true
        };

        if (dto.ImageUrls is { Count: > 0 })
        {
            var urls = dto.ImageUrls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Take(6)
                .ToList();

            for (var i = 0; i < urls.Count; i++)
            {
                review.ReviewImages.Add(new ReviewImage
                {
                    ImageUrl = urls[i],
                    SortOrder = i
                });
            }
        }

        await unitOfWork.Reviews.AddAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReviewDtoMapper.ToDto(review);
    }
}

