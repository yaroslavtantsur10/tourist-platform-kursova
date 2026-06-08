using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Hotels;

[Authorize(Roles = RoleNames.Administrator)]
public class DeleteModel : PageModel {
  private readonly ApplicationDbContext _context;

  public DeleteModel(ApplicationDbContext context) {
    _context = context;
  }

  public Hotel Hotel { get; set; } = new();

  public int ToursCount { get; set; }

  public bool CanDelete => ToursCount == 0;

  public async Task<IActionResult> OnGetAsync(int id) {
    var hotel = await _context.Hotels
        .Include(h => h.Country)
        .Include(h => h.Tours)
        .FirstOrDefaultAsync(h => h.Id == id);

    if (hotel == null) {
      TempData["ErrorMessage"] = "Готель не знайдено.";
      return RedirectToPage("Index");
    }

    Hotel = hotel;
    ToursCount = hotel.Tours.Count;

    return Page();
  }

  public async Task<IActionResult> OnPostAsync(int id) {
    var hotel = await _context.Hotels
        .Include(h => h.Tours)
        .FirstOrDefaultAsync(h => h.Id == id);

    if (hotel == null) {
      TempData["ErrorMessage"] = "Готель не знайдено.";
      return RedirectToPage("Index");
    }

    if (hotel.Tours.Any()) {
      TempData["ErrorMessage"] =
          "Неможливо видалити готель, оскільки він використовується у туристичних путівках.";

      return RedirectToPage("Index");
    }

    _context.Hotels.Remove(hotel);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Готель успішно видалено.";

    return RedirectToPage("Index");
  }
}