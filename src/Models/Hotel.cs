using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class Hotel {
  public int Id { get; set; }

  [Required(ErrorMessage = "Вкажіть назву готелю")]
  [MaxLength(150)]
  public string Name { get; set; } = string.Empty;

  [Range(1, 5, ErrorMessage = "Рейтинг готелю має бути від 1 до 5")]
  public int Rating { get; set; }

  public int CountryId { get; set; }

  public Country? Country { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  public ICollection<Tour> Tours { get; set; } = new List<Tour>();
}