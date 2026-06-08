using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.ViewModels;

public class TourFormViewModel {
  public int Id { get; set; }

  [Required(ErrorMessage = "Вкажіть назву туру")]
  [MaxLength(200)]
  [Display(Name = "Назва туру")]
  public string Title { get; set; } = string.Empty;

  [Required(ErrorMessage = "Вкажіть опис туру")]
  [MaxLength(3000)]
  [Display(Name = "Опис туру")]
  public string Description { get; set; } = string.Empty;

  [Range(1, 1000000, ErrorMessage = "Ціна має бути більшою за 0")]
  [Display(Name = "Ціна")]
  public decimal Price { get; set; }

  [Required(ErrorMessage = "Оберіть країну")]
  [Display(Name = "Країна")]
  public int CountryId { get; set; }

  [Required(ErrorMessage = "Оберіть готель")]
  [Display(Name = "Готель")]
  public int HotelId { get; set; }

  [DataType(DataType.Date)]
  [Display(Name = "Дата початку")]
  public DateTime StartDate { get; set; } = DateTime.Today.AddDays(7);

  [DataType(DataType.Date)]
  [Display(Name = "Дата завершення")]
  public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);

  [Range(0, 10000, ErrorMessage = "Кількість доступних місць не може бути від’ємною")]
  [Display(Name = "Кількість доступних місць")]
  public int AvailablePlaces { get; set; }

  [Display(Name = "Активний тур")]
  public bool IsActive { get; set; } = true;

  public string? ExistingPhotoPath { get; set; }

  [Display(Name = "Фото туру")]
  public IFormFile? PhotoFile { get; set; }
}