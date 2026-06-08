using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.ViewModels;

public class UserDashboardViewModel {
  public int FavoriteToursCount { get; set; }

  public int ReviewsCount { get; set; }

  public int RatedToursCount { get; set; }

  public int BookingsCount { get; set; }

  public IList<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();

  public IList<Booking> RecentBookings { get; set; } = new List<Booking>();

  public IList<Review> RecentReviews { get; set; } = new List<Review>();

  public IList<Tour> RecommendedTours { get; set; } = new List<Tour>();

  public bool HasRatingsForPersonalization { get; set; }
}