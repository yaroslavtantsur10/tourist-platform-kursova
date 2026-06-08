using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Countries;

[Authorize(Roles = RoleNames.Administrator)]
public class EditModel : PageModel {
  private readonly ApplicationDbContext _context;

  public EditModel(ApplicationDbContext context) {
    _context = context;
  }

  [BindProperty]
  public Country Country { get; set; } = new();

  public async Task<IActionResult> OnGetAsync(int id) {
    var country = await _context.Countries.FindAsync(id);

    if (country == null) {
      TempData["ErrorMessage"] = "Країну не знайдено.";
      return RedirectToPage("Index");
    }

    Country = country;

    return Page();
  }

  public async Task<IActionResult> OnPostAsync() {
    if (!ModelState.IsValid) {
      return Page();
    }

    bool nameExists = await _context.Countries
        .AnyAsync(c => c.Name == Country.Name && c.Id != Country.Id);

    if (nameExists) {
      ModelState.AddModelError("Country.Name", "Інша країна з такою назвою вже існує.");
      return Page();
    }

    var countryToUpdate = await _context.Countries.FindAsync(Country.Id);

    if (countryToUpdate == null) {
      TempData["ErrorMessage"] = "Країну не знайдено.";
      return RedirectToPage("Index");
    }

    countryToUpdate.Name = Country.Name;
    countryToUpdate.Description = Country.Description;

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Дані країни успішно оновлено.";

    return RedirectToPage("Index");
  }
}