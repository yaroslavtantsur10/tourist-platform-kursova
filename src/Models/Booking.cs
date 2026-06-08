using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class Booking {
  public int Id { get; set; }

  [Required]
  public string UserId { get; set; } = string.Empty;

  public ApplicationUser? User { get; set; }

  public int TourId { get; set; }

  public Tour? Tour { get; set; }

  public DateTime BookingDate { get; set; } = DateTime.Now;

  [Required]
  [MaxLength(50)]
  public string Status { get; set; } = BookingStatuses.Pending;
}

public static class BookingStatuses {
  public const string Pending = "Очікує";

  public const string Confirmed = "Підтверджено";

  public const string Canceled = "Скасовано";
}