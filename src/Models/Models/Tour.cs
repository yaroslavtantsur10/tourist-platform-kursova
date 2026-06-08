using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class Tour {
  public int Id { get; set; }

  [Required(ErrorMessage = "Вкажіть назву туру")]
  [MaxLength(200)]
  public string Title { get; set; } = string.Empty;

  [Required(ErrorMessage = "Вкажіть опис туру")]
  [MaxLength(3000)]
  public string Description { get; set; } = string.Empty;

  [Range(1, 1000000, ErrorMessage = "Ціна має бути більшою за 0")]
  public decimal Price { get; set; }

  public int CountryId { get; set; }

  public Country? Country { get; set; }

  public int HotelId { get; set; }

  public Hotel? Hotel { get; set; }

  [DataType(DataType.Date)]
  public DateTime StartDate { get; set; }

  [DataType(DataType.Date)]
  public DateTime EndDate { get; set; }

  [Range(0, 10000, ErrorMessage = "Кількість доступних місць не може бути від’ємною")]
  public int AvailablePlaces { get; set; }

  [MaxLength(500)]
  public string? PhotoPath { get; set; }

  public bool IsActive { get; set; } = true;

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

  public ICollection<Review> Reviews { get; set; } = new List<Review>();

  public ICollection<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();
}