using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class Country {
  public int Id { get; set; }

  [Required(ErrorMessage = "Вкажіть назву країни")]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;

  [MaxLength(1000)]
  public string? Description { get; set; }

  public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

  public ICollection<Tour> Tours { get; set; } = new List<Tour>();
}