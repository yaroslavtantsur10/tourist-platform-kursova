using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.Services;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Admin.Tours;

[Authorize(Roles = RoleNames.Administrator)]
public class CreateModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly FileUploadService _fileUploadService;

  public CreateModel(ApplicationDbContext context, FileUploadService fileUploadService) {
    _context = context;
    _fileUploadService = fileUploadService;
  }

  [BindProperty]
  public TourFormViewModel Input { get; set; } = new();

  public SelectList CountriesSelectList { get; set; } = default!;

  public SelectList HotelsSelectList { get; set; } = default!;

  public async Task OnGetAsync() {
    await LoadSelectListsAsync();
  }

  public async Task<IActionResult> OnPostAsync() {
    await LoadSelectListsAsync();

    if (Input.EndDate <= Input.StartDate) {
      ModelState.AddModelError("Input.EndDate", "Дата завершення має бути пізнішою за дату початку.");
    }

    bool countryExists = await _context.Countries.AnyAsync(c => c.Id == Input.CountryId);

    if (!countryExists) {
      ModelState.AddModelError("Input.CountryId", "Оберіть коректну країну.");
    }

    bool hotelExists = await _context.Hotels
        .AnyAsync(h => h.Id == Input.HotelId && h.CountryId == Input.CountryId);

    if (!hotelExists) {
      ModelState.AddModelError("Input.HotelId", "Оберіть готель, який належить вибраній країні.");
    }

    if (!ModelState.IsValid) {
      return Page();
    }

    string? photoPath;

    try {
      photoPath = await _fileUploadService.UploadTourPhotoAsync(Input.PhotoFile);
    } catch (InvalidOperationException ex) {
      ModelState.AddModelError("Input.PhotoFile", ex.Message);
      return Page();
    }

    var tour = new Tour {
      Title = Input.Title,
      Description = Input.Description,
      Price = Input.Price,
      CountryId = Input.CountryId,
      HotelId = Input.HotelId,
      StartDate = Input.StartDate,
      EndDate = Input.EndDate,
      AvailablePlaces = Input.AvailablePlaces,
      PhotoPath = photoPath ?? "/uploads/tours/default-tour.jpg",
      IsActive = Input.IsActive,
      CreatedAt = DateTime.Now
    };

    _context.Tours.Add(tour);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Туристичну путівку успішно додано.";

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