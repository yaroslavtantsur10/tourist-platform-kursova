using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Countries;

[Authorize(Roles = RoleNames.Administrator)]
public class CreateModel : PageModel {
  private readonly ApplicationDbContext _context;

  public CreateModel(ApplicationDbContext context) {
    _context = context;
  }

  [BindProperty]
  public Country Country { get; set; } = new();

  public void OnGet() {
  }

  public async Task<IActionResult> OnPostAsync() {
    if (!ModelState.IsValid) {
      return Page();
    }

    bool exists = _context.Countries.Any(c => c.Name == Country.Name);

    if (exists) {
      ModelState.AddModelError("Country.Name", "Країна з такою назвою вже існує.");
      return Page();
    }

    _context.Countries.Add(Country);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Країну успішно додано.";

    return RedirectToPage("Index");
  }
}