using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Reviews;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public IList<Review> Reviews { get; set; } = new List<Review>();

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task OnGetAsync() {
    await LoadReviewsAsync();
  }

  public async Task<IActionResult> OnPostDeleteAsync(int id) {
    var review = await _context.Reviews.FindAsync(id);

    if (review == null) {
      ErrorMessage = "Відгук не знайдено.";
      return RedirectToPage();
    }

    _context.Reviews.Remove(review);
    await _context.SaveChangesAsync();

    SuccessMessage = "Відгук успішно видалено.";

    return RedirectToPage();
  }

  private async Task LoadReviewsAsync() {
    Reviews = await _context.Reviews
        .Include(r => r.User)
        .Include(r => r.Tour)
            .ThenInclude(t => t!.Country)
        .Include(r => r.Tour)
            .ThenInclude(t => t!.Hotel)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();
  }
}