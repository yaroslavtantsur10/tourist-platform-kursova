using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.ViewModels;

public class ReviewInputViewModel {
  [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
  [Display(Name = "Оцінка туру")]
  public int Rating { get; set; } = 5;

  [MaxLength(2000, ErrorMessage = "Коментар не може перевищувати 2000 символів")]
  [Display(Name = "Коментар до туру, необов’язково")]
  public string? Comment { get; set; }
}