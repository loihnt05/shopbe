namespace Shopbe.Application.User.UserAddresses.Dtos;

public class UserAddressQueryDto
{
    public Guid UserId { get; set; }

    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public bool? IsDefault { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20; 
}   