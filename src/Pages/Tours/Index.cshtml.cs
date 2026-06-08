using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Tours;

public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public IList<Tour> Tours { get; set; } = new List<Tour>();

  [BindProperty(SupportsGet = true)]
  public TourCatalogFilterViewModel Filter { get; set; } = new();

  public SelectList CountriesSelectList { get; set; } = default!;

  public SelectList HotelsSelectList { get; set; } = default!;

  public async Task OnGetAsync() {
    await LoadSelectListsAsync();

    IQueryable<Tour> query = _context.Tours
        .Include(t => t.Country)
        .Include(t => t.Hotel)
        .Include(t => t.Reviews)
        .Where(t => t.IsActive);

    if (!string.IsNullOrWhiteSpace(Filter.SearchTerm)) {
      string searchTerm = Filter.SearchTerm.Trim();

      query = query.Where(t =>
          t.Title.Contains(searchTerm) ||
          t.Description.Contains(searchTerm) ||
          t.Country!.Name.Contains(searchTerm) ||
          t.Hotel!.Name.Contains(searchTerm));
    }

    if (Filter.CountryId.HasValue) {
      query = query.Where(t => t.CountryId == Filter.CountryId.Value);
    }

    if (Filter.HotelId.HasValue) {
      query = query.Where(t => t.HotelId == Filter.HotelId.Value);
    }

    if (Filter.MinPrice.HasValue) {
      query = query.Where(t => t.Price >= Filter.MinPrice.Value);
    }

    if (Filter.MaxPrice.HasValue) {
      query = query.Where(t => t.Price <= Filter.MaxPrice.Value);
    }

    if (Filter.MinHotelRating.HasValue) {
      query = query.Where(t => t.Hotel != null && t.Hotel.Rating >= Filter.MinHotelRating.Value);
    }

    if (Filter.StartDateFrom.HasValue) {
      query = query.Where(t => t.StartDate >= Filter.StartDateFrom.Value);
    }

    if (Filter.EndDateTo.HasValue) {
      query = query.Where(t => t.EndDate <= Filter.EndDateTo.Value);
    }

    if (Filter.OnlyAvailable) {
      query = query.Where(t => t.AvailablePlaces > 0);
    }

    Tours = await query
        .OrderBy(t => t.StartDate)
        .ThenBy(t => t.Price)
        .ToListAsync();
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