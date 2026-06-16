using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LaBikeLifeDotNet.Models;

// une moto ajoutée par l'utilisateur dans son garage
public class UserMotorcycle
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
    public IdentityUser? User { get; set; }

    [Required, MaxLength(100)]
    public string Make { get; set; } = default!;

    [Required, MaxLength(150)]
    public string Model { get; set; } = default!;

    [Range(1980, 2100)]
    public int Year { get; set; }

    [MaxLength(17)]
    public string? Vin { get; set; }

    [MaxLength(600)]
    public string? ImageUrl { get; set; }

    // rempli depuis le VIN quand on en a un, souvent en partie seulement
    public int? DisplacementCc { get; set; }
    public int? Cylinders { get; set; }

    [MaxLength(60)]
    public string? FuelType { get; set; }

    [MaxLength(120)]
    public string? BrakeSystem { get; set; }

    [MaxLength(120)]
    public string? BodyClass { get; set; }

    public int CurrentMileageKm { get; set; }

    public DateTime MileageUpdatedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }
}
