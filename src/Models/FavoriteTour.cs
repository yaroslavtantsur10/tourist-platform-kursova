using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class FavoriteTour {
  public int Id { get; set; }

  [Required]
  public string UserId { get; set; } = string.Empty;

  public ApplicationUser? User { get; set; }

  public int TourId { get; set; }

  public Tour? Tour { get; set; }

  public DateTime AddedAt { get; set; } = DateTime.Now;
}