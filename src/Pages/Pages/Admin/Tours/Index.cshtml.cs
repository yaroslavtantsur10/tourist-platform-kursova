using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Tours;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public IList<Tour> Tours { get; set; } = new List<Tour>();

  public async Task OnGetAsync() {
    Tours = await _context.Tours
        .Include(t => t.Country)
        .Include(t => t.Hotel)
        .Include(t => t.Bookings)
        .Include(t => t.Reviews)
        .Include(t => t.FavoriteTours)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
  }
}