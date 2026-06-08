using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class Review {
  public int Id { get; set; }

  [Required]
  public string UserId { get; set; } = string.Empty;

  public ApplicationUser? User { get; set; }

  public int TourId { get; set; }

  public Tour? Tour { get; set; }

  [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
  public int Rating { get; set; }

  [MaxLength(2000)]
  public string? Comment { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.Now;
}