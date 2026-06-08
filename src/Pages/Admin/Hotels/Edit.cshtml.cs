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
public class EditModel : PageModel {
  private readonly ApplicationDbContext _context;

  public EditModel(ApplicationDbContext context) {
    _context = context;
  }

  [BindProperty]
  public Hotel Hotel { get; set; } = new();

  public SelectList CountriesSelectList { get; set; } = default!;

  public async Task<IActionResult> OnGetAsync(int id) {
    var hotel = await _context.Hotels.FindAsync(id);

    if (hotel == null) {
      TempData["ErrorMessage"] = "Готель не знайдено.";
      return RedirectToPage("Index");
    }

    Hotel = hotel;

    await LoadCountriesAsync();

    return Page();
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
        .AnyAsync(h => h.Name == Hotel.Name &&
                       h.CountryId == Hotel.CountryId &&
                       h.Id != Hotel.Id);

    if (hotelExists) {
      ModelState.AddModelError("Hotel.Name", "Інший готель із такою назвою вже існує в обраній країні.");
      return Page();
    }

    var hotelToUpdate = await _context.Hotels.FindAsync(Hotel.Id);

    if (hotelToUpdate == null) {
      TempData["ErrorMessage"] = "Готель не знайдено.";
      return RedirectToPage("Index");
    }

    hotelToUpdate.Name = Hotel.Name;
    hotelToUpdate.CountryId = Hotel.CountryId;
    hotelToUpdate.Rating = Hotel.Rating;
    hotelToUpdate.Description = Hotel.Description;

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Дані готелю успішно оновлено.";

    return RedirectToPage("Index");
  }

  private async Task LoadCountriesAsync() {
    var countries = await _context.Countries
        .OrderBy(c => c.Name)
        .ToListAsync();

    CountriesSelectList = new SelectList(countries, "Id", "Name");
  }
}