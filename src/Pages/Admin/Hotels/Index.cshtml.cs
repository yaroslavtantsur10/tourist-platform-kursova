using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Hotels;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public IList<Hotel> Hotels { get; set; } = new List<Hotel>();

  public async Task OnGetAsync() {
    Hotels = await _context.Hotels
        .Include(h => h.Country)
        .Include(h => h.Tours)
        .OrderBy(h => h.Country!.Name)
        .ThenBy(h => h.Name)
        .ToListAsync();
  }
}