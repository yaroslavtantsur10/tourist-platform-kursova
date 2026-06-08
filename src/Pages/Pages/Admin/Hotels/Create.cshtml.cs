using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Hotels;

[Authorize(Roles = RoleNames.Administrator)]
public class CreateModel : PageModel {
  private readonly ApplicationDbContext _context;

  public CreateModel(ApplicationDbContext context) {
    _context = context;
  }

  [BindProperty]
  public Hotel Hotel { get; set; } = new();

  public SelectList CountriesSelectList { get; set; } = default!;

  public async Task OnGetAsync() {
    await LoadCountriesAsync();
  }

  public async Task<IActionResult> OnPostAsync() {
    await LoadCountriesAsync();

    if (!ModelState.IsValid) {
      return Page();
    }

    bool countryExists = await _context.Countries
        .AnyAsync(c => c.Id == Hotel.CountryId);

    if (!countryExists) {
      ModelState.AddModelError("Hotel.CountryId", "Оберіть коректну країну.");
      return Page();
    }

    bool hotelExists = await _context.Hotels
        .AnyAsync(h => h.Name == Hotel.Name && h.CountryId == Hotel.CountryId);

    if (hotelExists) {
      ModelState.AddModelError("Hotel.Name", "Готель із такою назвою вже існує в обраній країні.");
      return Page();
    }

    _context.Hotels.Add(Hotel);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Готель успішно додано.";

    return RedirectToPage("Index");
  }

  private async Task LoadCountriesAsync() {
    var countries = await _context.Countries
        .OrderBy(c => c.Name)
        .ToListAsync();

    CountriesSelectList = new SelectList(countries, "Id", "Name");
  }
}