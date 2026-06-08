using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Tours;

public class DetailsModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public DetailsModel(
      ApplicationDbContext context,
      UserManager<ApplicationUser> userManager) {
    _context = context;
    _userManager = userManager;
  }

  public Tour Tour { get; set; } = new();

  public double AverageRating { get; set; }

  public int ReviewsCount { get; set; }

  public bool CurrentUserHasReview { get; set; }

  [BindProperty]
  public ReviewInputViewModel ReviewInput { get; set; } = new();

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task<IActionResult> OnGetAsync(int id) {
    var result = await LoadTourAsync(id);

    if (result != null) {
      return result;
    }

    await LoadCurrentUserReviewAsync();

    return Page();
  }

  public async Task<IActionResult> OnPostReviewAsync(int id) {
    var result = await LoadTourAsync(id);

    if (result != null) {
      return result;
    }

    var user = await _userManager.GetUserAsync(User);

    if (user == null) {
      return Challenge();
    }

    if (user.IsBlocked) {
      ErrorMessage = "Ваш обліковий запис заблоковано. Додавання відгуків недоступне.";
      return RedirectToPage(new { id });
    }

    if (!ModelState.IsValid) {
      await LoadCurrentUserReviewAsync();
      return Page();
    }

    var existingReview = await _context.Reviews
        .FirstOrDefaultAsync(r => r.UserId == user.Id && r.TourId == id);

    if (existingReview == null) {
      var review = new Review {
        UserId = user.Id,
        TourId = id,
        Rating = ReviewInput.Rating,
        Comment = ReviewInput.Comment,
        CreatedAt = DateTime.Now
      };

      _context.Reviews.Add(review);

      SuccessMessage = "Відгук успішно додано.";
    } else {
      existingReview.Rating = ReviewInput.Rating;
      existingReview.Comment = ReviewInput.Comment;
      existingReview.CreatedAt = DateTime.Now;

      SuccessMessage = "Ваш попередній відгук успішно оновлено.";
    }

    await _context.SaveChangesAsync();

    return RedirectToPage(new { id });
  }

  private async Task<IActionResult?> LoadTourAsync(int id) {
    var tour = await _context.Tours
        .Include(t => t.Country)
        .Include(t => t.Hotel)
        .Include(t => t.Reviews)
            .ThenInclude(r => r.User)
        .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

    if (tour == null) {
      return NotFound();
    }

    Tour = tour;

    ReviewsCount = tour.Reviews.Count;
    AverageRating = tour.Reviews.Any()
        ? tour.Reviews.Average(r => r.Rating)
        : 0;

    return null;
  }

  private async Task LoadCurrentUserReviewAsync() {
    if (User.Identity?.IsAuthenticated != true) {
      return;
    }

    var userId = _userManager.GetUserId(User);

    var currentUserReview = await _context.Reviews
        .FirstOrDefaultAsync(r => r.UserId == userId && r.TourId == Tour.Id);

    if (currentUserReview != null) {
      CurrentUserHasReview = true;

      ReviewInput = new ReviewInputViewModel {
        Rating = currentUserReview.Rating,
        Comment = currentUserReview.Comment
      };
    }
  }
}