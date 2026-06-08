using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.ViewModels;

public class TourCatalogFilterViewModel {
  [Display(Name = "Пошук")]
  public string? SearchTerm { get; set; }

  [Display(Name = "Країна")]
  public int? CountryId { get; set; }

  [Display(Name = "Готель")]
  public int? HotelId { get; set; }

  [Display(Name = "Мінімальна ціна")]
  public decimal? MinPrice { get; set; }

  [Display(Name = "Максимальна ціна")]
  public decimal? MaxPrice { get; set; }

  [Display(Name = "Мінімальний рейтинг готелю")]
  public int? MinHotelRating { get; set; }

  [DataType(DataType.Date)]
  [Display(Name = "Дата початку від")]
  public DateTime? StartDateFrom { get; set; }

  [DataType(DataType.Date)]
  [Display(Name = "Дата завершення до")]
  public DateTime? EndDateTo { get; set; }

  [Display(Name = "Лише з доступними місцями")]
  public bool OnlyAvailable { get; set; }
}