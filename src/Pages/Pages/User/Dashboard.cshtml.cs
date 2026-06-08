using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.Services;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.User;

[Authorize]
public class DashboardModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly RecommendationService _recommendationService;

  public DashboardModel(
      ApplicationDbContext context,
      UserManager<ApplicationUser> userManager,
      RecommendationService recommendationService) {
    _context = context;
    _userManager = userManager;
    _recommendationService = recommendationService;
  }

  public UserDashboardViewModel Dashboard { get; set; } = new();

  public async Task OnGetAsync() {
    string? userId = _userManager.GetUserId(User);

    if (string.IsNullOrWhiteSpace(userId)) {
      return;
    }

    int reviewsCount = await _context.Reviews
        .CountAsync(r => r.UserId == userId);

    int ratedToursCount = await _context.Reviews
        .Where(r => r.UserId == userId)
        .Select(r => r.TourId)
        .Distinct()
        .CountAsync();

    var favoriteTours = await _context.FavoriteTours
        .Include(f => f.Tour)
            .ThenInclude(t => t!.Country)
        .Include(f => f.Tour)
            .ThenInclude(t => t!.Hotel)
        .Include(f => f.Tour)
            .ThenInclude(t => t!.Reviews)
        .Where(f => f.UserId == userId)
        .OrderByDescending(f => f.AddedAt)
        .Take(3)
        .ToListAsync();

    var recentBookings = await _context.Bookings
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Country)
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Hotel)
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.BookingDate)
        .Take(5)
        .ToListAsync();

    var recentReviews = await _context.Reviews
        .Include(r => r.Tour)
            .ThenInclude(t => t!.Country)
        .Include(r => r.Tour)
            .ThenInclude(t => t!.Hotel)
        .Where(r => r.UserId == userId)
        .OrderByDescending(r => r.CreatedAt)
        .Take(5)
        .ToListAsync();

    var ratedTourIds = await _context.Reviews
        .Where(r => r.UserId == userId)
        .Select(r => r.TourId)
        .ToListAsync();

    var candidateTours = await _context.Tours
        .Include(t => t.Country)
        .Include(t => t.Hotel)
        .Include(t => t.Reviews)
        .Where(t => t.IsActive && !ratedTourIds.Contains(t.Id))
        .ToListAsync();

    IList<Tour> recommendedTours;

    if (_recommendationService.ModelExists() && ratedTourIds.Any()) {
      var predictedScores = _recommendationService.PredictForUser(
          userId,
          candidateTours.Select(t => t.Id));

      var orderedTourIds = predictedScores
          .Select(p => p.TourId)
          .Take(3)
          .ToList();

      recommendedTours = candidateTours
          .Where(t => orderedTourIds.Contains(t.Id))
          .OrderBy(t => orderedTourIds.IndexOf(t.Id))
          .ToList();
    } else {
      recommendedTours = candidateTours
          .OrderByDescending(t => t.Reviews.Any()
              ? t.Reviews.Average(r => r.Rating)
              : 0)
          .ThenByDescending(t => t.CreatedAt)
          .Take(3)
          .ToList();
    }

    Dashboard = new UserDashboardViewModel {
      FavoriteToursCount = await _context.FavoriteTours.CountAsync(f => f.UserId == userId),
      ReviewsCount = reviewsCount,
      RatedToursCount = ratedToursCount,
      BookingsCount = await _context.Bookings.CountAsync(b => b.UserId == userId),
      FavoriteTours = favoriteTours,
      RecentBookings = recentBookings,
      RecentReviews = recentReviews,
      RecommendedTours = recommendedTours,
      HasRatingsForPersonalization = reviewsCount > 0
    };
  }
}