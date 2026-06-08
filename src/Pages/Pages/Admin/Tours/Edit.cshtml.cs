using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Services;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Admin.Tours;

[Authorize(Roles = RoleNames.Administrator)]
public class EditModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly FileUploadService _fileUploadService;

  public EditModel(ApplicationDbContext context, FileUploadService fileUploadService) {
    _context = context;
    _fileUploadService = fileUploadService;
  }

  [BindProperty]
  public TourFormViewModel Input { get; set; } = new();

  public SelectList CountriesSelectList { get; set; } = default!;

  public SelectList HotelsSelectList { get; set; } = default!;

  public async Task<IActionResult> OnGetAsync(int id) {
    var tour = await _context.Tours.FindAsync(id);

    if (tour == null) {
      TempData["ErrorMessage"] = "Тур не знайдено.";
      return RedirectToPage("Index");
    }

    Input = new TourFormViewModel {
      Id = tour.Id,
      Title = tour.Title,
      Description = tour.Description,
      Price = tour.Price,
      CountryId = tour.CountryId,
      HotelId = tour.HotelId,
      StartDate = tour.StartDate,
      EndDate = tour.EndDate,
      AvailablePlaces = tour.AvailablePlaces,
      IsActive = tour.IsActive,
      ExistingPhotoPath = tour.PhotoPath
    };

    await LoadSelectListsAsync();

    return Page();
  }

  public async Task<IActionResult> OnPostAsync() {
    await LoadSelectListsAsync();

    if (Input.EndDate <= Input.StartDate) {
      ModelState.AddModelError("Input.EndDate", "Дата завершення має бути пізнішою за дату початку.");
    }

    bool hotelExists = await _context.Hotels
        .AnyAsync(h => h.Id == Input.HotelId && h.CountryId == Input.CountryId);

    if (!hotelExists) {
      ModelState.AddModelError("Input.HotelId", "Оберіть готель, який належить вибраній країні.");
    }

    if (!ModelState.IsValid) {
      return Page();
    }

    var tourToUpdate = await _context.Tours.FindAsync(Input.Id);

    if (tourToUpdate == null) {
      TempData["ErrorMessage"] = "Тур не знайдено.";
      return RedirectToPage("Index");
    }

    string? newPhotoPath = null;

    try {
      newPhotoPath = await _fileUploadService.UploadTourPhotoAsync(Input.PhotoFile);
    } catch (InvalidOperationException ex) {
      ModelState.AddModelError("Input.PhotoFile", ex.Message);
      return Page();
    }

    if (!string.IsNullOrWhiteSpace(newPhotoPath)) {
      _fileUploadService.DeletePhotoIfExists(tourToUpdate.PhotoPath);
      tourToUpdate.PhotoPath = newPhotoPath;
    }

    tourToUpdate.Title = Input.Title;
    tourToUpdate.Description = Input.Description;
    tourToUpdate.Price = Input.Price;
    tourToUpdate.CountryId = Input.CountryId;
    tourToUpdate.HotelId = Input.HotelId;
    tourToUpdate.StartDate = Input.StartDate;
    tourToUpdate.EndDate = Input.EndDate;
    tourToUpdate.AvailablePlaces = Input.AvailablePlaces;
    tourToUpdate.IsActive = Input.IsActive;

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Дані туристичної путівки успішно оновлено.";

    return RedirectToPage("Index");
  }

  private async Task LoadSelectListsAsync() {
    var countries = await _context.Countries
        .OrderBy(c => c.Name)
        .ToListAsync();

    var hotels = await _context.Hotels
        .Include(h => h.Country)
        .OrderBy(h => h.Country!.Name)
        .ThenBy(h => h.Name)
        .ToListAsync();

    CountriesSelectList = new SelectList(countries, "Id", "Name");

    HotelsSelectList = new SelectList(
        hotels.Select(h => new {
          h.Id,
          Name = $"{h.Name} ({h.Country!.Name}, {h.Rating}★)"
        }),
        "Id",
        "Name");
  }
}