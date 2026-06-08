using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.Services;

namespace TourRecommenderPlatform.Pages.Admin.Tours;

[Authorize(Roles = RoleNames.Administrator)]
public class DeleteModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly FileUploadService _fileUploadService;

  public DeleteModel(ApplicationDbContext context, FileUploadService fileUploadService) {
    _context = context;
    _fileUploadService = fileUploadService;
  }

  public Tour Tour { get; set; } = new();

  public int BookingsCount { get; set; }

  public int ReviewsCount { get; set; }

  public int FavoritesCount { get; set; }

  public bool HasRelatedData => BookingsCount > 0 || ReviewsCount > 0 || FavoritesCount > 0;

  public async Task<IActionResult> OnGetAsync(int id) {
    var tour = await _context.Tours
        .Include(t => t.Country)
        .Include(t => t.Hotel)
        .Include(t => t.Bookings)
        .Include(t => t.Reviews)
        .Include(t => t.FavoriteTours)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (tour == null) {
      TempData["ErrorMessage"] = "Тур не знайдено.";
      return RedirectToPage("Index");
    }

    Tour = tour;
    BookingsCount = tour.Bookings.Count;
    ReviewsCount = tour.Reviews.Count;
    FavoritesCount = tour.FavoriteTours.Count;

    return Page();
  }

  public async Task<IActionResult> OnPostDeleteAsync(int id) {
    var tour = await _context.Tours
        .Include(t => t.Bookings)
        .Include(t => t.Reviews)
        .Include(t => t.FavoriteTours)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (tour == null) {
      TempData["ErrorMessage"] = "Тур не знайдено.";
      return RedirectToPage("Index");
    }

    if (tour.Bookings.Any() || tour.Reviews.Any() || tour.FavoriteTours.Any()) {
      TempData["ErrorMessage"] =
          "Неможливо фізично видалити тур, оскільки він має пов’язані бронювання, відгуки або записи в улюбленому.";

      return RedirectToPage("Index");
    }

    _fileUploadService.DeletePhotoIfExists(tour.PhotoPath);

    _context.Tours.Remove(tour);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Тур успішно видалено.";

    return RedirectToPage("Index");
  }

  public async Task<IActionResult> OnPostDeactivateAsync(int id) {
    var tour = await _context.Tours.FindAsync(id);

    if (tour == null) {
      TempData["ErrorMessage"] = "Тур не знайдено.";
      return RedirectToPage("Index");
    }

    tour.IsActive = false;
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Тур деактивовано. Він більше не буде відображатися у користувацькому каталозі.";

    return RedirectToPage("Index");
  }
}