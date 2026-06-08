namespace TourRecommenderPlatform.ViewModels;

public class AdminStatisticsViewModel {
  public IList<CountryPopularityItem> CountryPopularity { get; set; } = new List<CountryPopularityItem>();

  public IList<TourRatingItem> TourRatings { get; set; } = new List<TourRatingItem>();

  public IList<TourReviewsCountItem> TourReviewsCounts { get; set; } = new List<TourReviewsCountItem>();

  public int TotalBookings { get; set; }

  public int TotalReviews { get; set; }

  public int TotalRatedTours { get; set; }

  public double AverageSystemRating { get; set; }
}

public class CountryPopularityItem {
  public string CountryName { get; set; } = string.Empty;

  public int BookingsCount { get; set; }
}

public class TourRatingItem {
  public string TourTitle { get; set; } = string.Empty;

  public double AverageRating { get; set; }
}

public class TourReviewsCountItem {
  public string TourTitle { get; set; } = string.Empty;

  public int ReviewsCount { get; set; }
}