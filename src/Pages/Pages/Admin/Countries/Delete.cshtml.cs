using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Countries;

[Authorize(Roles = RoleNames.Administrator)]
public class DeleteModel : PageModel {
  private readonly ApplicationDbContext _context;

  public DeleteModel(ApplicationDbContext context) {
    _context = context;
  }

  public Country Country { get; set; } = new();

  public int HotelsCount { get; set; }

  public int ToursCount { get; set; }

  public bool CanDelete => HotelsCount == 0 && ToursCount == 0;

  public async Task<IActionResult> OnGetAsync(int id) {
    var country = await _context.Countries
        .Include(c => c.Hotels)
        .Include(c => c.Tours)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (country == null) {
      TempData["ErrorMessage"] = "Країну не знайдено.";
      return RedirectToPage("Index");
    }

    Country = country;
    HotelsCount = country.Hotels.Count;
    ToursCount = country.Tours.Count;

    return Page();
  }

  public async Task<IActionResult> OnPostAsync(int id) {
    var country = await _context.Countries
        .Include(c => c.Hotels)
        .Include(c => c.Tours)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (country == null) {
      TempData["ErrorMessage"] = "Країну не знайдено.";
      return RedirectToPage("Index");
    }

    if (country.Hotels.Any() || country.Tours.Any()) {
      TempData["ErrorMessage"] =
          "Неможливо видалити країну, оскільки вона пов’язана з готелями або турами.";

      return RedirectToPage("Index");
    }

    _context.Countries.Remove(country);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Країну успішно видалено.";

    return RedirectToPage("Index");
  }
}