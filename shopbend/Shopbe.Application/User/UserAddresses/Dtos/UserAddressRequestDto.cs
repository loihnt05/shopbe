using System.ComponentModel.DataAnnotations;

namespace Shopbe.Application.User.UserAddresses.Dtos;

public class UserAddressRequestDto
{
    [Required]
    [StringLength(200)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Ward { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}