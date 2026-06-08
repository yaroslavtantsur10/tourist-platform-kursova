using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Admin.Dashboard;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public AdminDashboardViewModel Dashboard { get; set; } = new();

  public async Task OnGetAsync() {
    var lastTraining = await _context.ModelTrainingLogs
        .OrderByDescending(x => x.TrainingDate)
        .FirstOrDefaultAsync();

    Dashboard = new AdminDashboardViewModel {
      TotalUsers = await _context.Users.CountAsync(),
      ActiveUsers = await _context.Users.CountAsync(u => !u.IsBlocked),
      BlockedUsers = await _context.Users.CountAsync(u => u.IsBlocked),

      TotalTours = await _context.Tours.CountAsync(),
      ActiveTours = await _context.Tours.CountAsync(t => t.IsActive),
      InactiveTours = await _context.Tours.CountAsync(t => !t.IsActive),

      TotalCountries = await _context.Countries.CountAsync(),
      TotalHotels = await _context.Hotels.CountAsync(),
      TotalBookings = await _context.Bookings.CountAsync(),
      TotalReviews = await _context.Reviews.CountAsync(),

      RatingsForTraining = await _context.Reviews.CountAsync(r => r.Rating >= 1 && r.Rating <= 5),

      LastTrainingDate = lastTraining?.TrainingDate,
      LastRmse = lastTraining?.Rmse,
      LastMae = lastTraining?.Mae,
      LastTrainingStatus = lastTraining?.Status
    };
  }
}