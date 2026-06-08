namespace TourRecommenderPlatform.ViewModels;

public class AdminDashboardViewModel {
  public int TotalUsers { get; set; }

  public int ActiveUsers { get; set; }

  public int BlockedUsers { get; set; }

  public int TotalTours { get; set; }

  public int ActiveTours { get; set; }

  public int InactiveTours { get; set; }

  public int TotalCountries { get; set; }

  public int TotalHotels { get; set; }

  public int TotalBookings { get; set; }

  public int TotalReviews { get; set; }

  public int RatingsForTraining { get; set; }

  public DateTime? LastTrainingDate { get; set; }

  public double? LastRmse { get; set; }

  public double? LastMae { get; set; }

  public string? LastTrainingStatus { get; set; }
}