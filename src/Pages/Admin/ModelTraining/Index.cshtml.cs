using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.Services;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Admin.ModelTraining;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly RecommendationService _recommendationService;
  private readonly UserManager<ApplicationUser> _userManager;

  public IndexModel(
      ApplicationDbContext context,
      RecommendationService recommendationService,
      UserManager<ApplicationUser> userManager) {
    _context = context;
    _recommendationService = recommendationService;
    _userManager = userManager;
  }

  public IList<ModelTrainingLog> TrainingLogs { get; set; } = new List<ModelTrainingLog>();

  public int ReviewsCount { get; set; }

  public int UsersWithRatingsCount { get; set; }

  public int RatedToursCount { get; set; }

  public bool ModelExists { get; set; }

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task OnGetAsync() {
    await LoadPageDataAsync();
  }

  public async Task<IActionResult> OnPostTrainAsync() {
    var admin = await _userManager.GetUserAsync(User);

    if (admin == null) {
      return Challenge();
    }

    ModelTrainingResultViewModel result = await _recommendationService.TrainModelAsync(admin.Id);

    if (result.IsSuccess) {
      SuccessMessage =
          $"Модель успішно навчено. Записів: {result.RecordsCount}, RMSE: {result.Rmse:F6}, MAE: {result.Mae:F6}.";
    } else {
      ErrorMessage = result.Message;
    }

    return RedirectToPage();
  }

  private async Task LoadPageDataAsync() {
    ReviewsCount = await _context.Reviews.CountAsync();

    UsersWithRatingsCount = await _context.Reviews
        .Select(r => r.UserId)
        .Distinct()
        .CountAsync();

    RatedToursCount = await _context.Reviews
        .Select(r => r.TourId)
        .Distinct()
        .CountAsync();

    ModelExists = _recommendationService.ModelExists();

    TrainingLogs = await _context.ModelTrainingLogs
        .Include(l => l.User)
        .OrderByDescending(l => l.TrainingDate)
        .Take(20)
        .ToListAsync();
  }
}