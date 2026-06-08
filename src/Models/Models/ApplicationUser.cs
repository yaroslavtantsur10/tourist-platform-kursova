using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TourRecommenderPlatform.Models;

public class ApplicationUser : IdentityUser {
  [MaxLength(150)]
  public string? FullName { get; set; }

  public DateTime RegisteredAt { get; set; } = DateTime.Now;

  public bool IsBlocked { get; set; } = false;

  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

  public ICollection<Review> Reviews { get; set; } = new List<Review>();

  public ICollection<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();

  public ICollection<ModelTrainingLog> ModelTrainingLogs { get; set; } = new List<ModelTrainingLog>();
}