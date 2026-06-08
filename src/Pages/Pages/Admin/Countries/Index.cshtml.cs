using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Countries;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public IList<Country> Countries { get; set; } = new List<Country>();

  public async Task OnGetAsync() {
    Countries = await _context.Countries
        .Include(c => c.Hotels)
        .Include(c => c.Tours)
        .OrderBy(c => c.Name)
        .ToListAsync();
  }
}