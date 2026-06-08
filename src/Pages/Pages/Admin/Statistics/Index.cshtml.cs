using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Admin.Statistics;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public AdminStatisticsViewModel Statistics { get; set; } = new();

  public string CountryPopularityLabelsJson { get; set; } = "[]";

  public string CountryPopularityValuesJson { get; set; } = "[]";

  public string TourRatingLabelsJson { get; set; } = "[]";

  public string TourRatingValuesJson { get; set; } = "[]";

  public string TourReviewsLabelsJson { get; set; } = "[]";

  public string TourReviewsValuesJson { get; set; } = "[]";

  public async Task OnGetAsync() {
    var countryPopularity = await _context.Bookings
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Country)
        .Where(b => b.Tour != null && b.Tour.Country != null)
        .GroupBy(b => b.Tour!.Country!.Name)
        .Select(g => new CountryPopularityItem {
          CountryName = g.Key,
          BookingsCount = g.Count()
        })
        .OrderByDescending(x => x.BookingsCount)
        .Take(10)
        .ToListAsync();

    var tourRatings = await _context.Reviews
        .Include(r => r.Tour)
        .Where(r => r.Tour != null)
        .GroupBy(r => r.Tour!.Title)
        .Select(g => new TourRatingItem {
          TourTitle = g.Key,
          AverageRating = g.Average(r => r.Rating)
        })
        .OrderByDescending(x => x.AverageRating)
        .Take(10)
        .ToListAsync();

    var tourReviewsCounts = await _context.Reviews
        .Include(r => r.Tour)
        .Where(r => r.Tour != null)
        .GroupBy(r => r.Tour!.Title)
        .Select(g => new TourReviewsCountItem {
          TourTitle = g.Key,
          ReviewsCount = g.Count()
        })
        .OrderByDescending(x => x.ReviewsCount)
        .Take(10)
        .ToListAsync();

    int totalReviews = await _context.Reviews.CountAsync();

    Statistics = new AdminStatisticsViewModel {
      CountryPopularity = countryPopularity,
      TourRatings = tourRatings,
      TourReviewsCounts = tourReviewsCounts,
      TotalBookings = await _context.Bookings.CountAsync(),
      TotalReviews = totalReviews,
      TotalRatedTours = await _context.Reviews
            .Select(r => r.TourId)
            .Distinct()
            .CountAsync(),
      AverageSystemRating = totalReviews > 0
            ? await _context.Reviews.AverageAsync(r => r.Rating)
            : 0
    };

    CountryPopularityLabelsJson = JsonSerializer.Serialize(
        countryPopularity.Select(x => x.CountryName));

    CountryPopularityValuesJson = JsonSerializer.Serialize(
        countryPopularity.Select(x => x.BookingsCount));

    TourRatingLabelsJson = JsonSerializer.Serialize(
        tourRatings.Select(x => x.TourTitle));

    TourRatingValuesJson = JsonSerializer.Serialize(
        tourRatings.Select(x => Math.Round(x.AverageRating, 2)));

    TourReviewsLabelsJson = JsonSerializer.Serialize(
        tourReviewsCounts.Select(x => x.TourTitle));

    TourReviewsValuesJson = JsonSerializer.Serialize(
        tourReviewsCounts.Select(x => x.ReviewsCount));
  }
}