using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Favorites;

[Authorize]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public IndexModel(
      ApplicationDbContext context,
      UserManager<ApplicationUser> userManager) {
    _context = context;
    _userManager = userManager;
  }

  public IList<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task OnGetAsync() {
    await LoadFavoritesAsync();
  }

  public async Task<IActionResult> OnPostAddAsync(int tourId) {
    var user = await _userManager.GetUserAsync(User);

    if (user == null) {
      return Challenge();
    }

    if (user.IsBlocked) {
      ErrorMessage = "Ваш обліковий запис заблоковано. Додавання турів до улюблених недоступне.";
      return RedirectToPage("/Tours/Details", new { id = tourId });
    }

    var tour = await _context.Tours
        .FirstOrDefaultAsync(t => t.Id == tourId && t.IsActive);

    if (tour == null) {
      ErrorMessage = "Тур не знайдено або він більше не є активним.";
      return RedirectToPage("/Tours/Index");
    }

    bool alreadyExists = await _context.FavoriteTours
        .AnyAsync(f => f.UserId == user.Id && f.TourId == tourId);

    if (alreadyExists) {
      ErrorMessage = "Цей тур уже додано до списку улюблених.";
      return RedirectToPage("/Tours/Details", new { id = tourId });
    }

    var favorite = new FavoriteTour {
      UserId = user.Id,
      TourId = tourId,
      AddedAt = DateTime.Now
    };

    _context.FavoriteTours.Add(favorite);
    await _context.SaveChangesAsync();

    SuccessMessage = "Тур успішно додано до улюблених.";

    return RedirectToPage("/Tours/Details", new { id = tourId });
  }

  public async Task<IActionResult> OnPostRemoveAsync(int id) {
    var user = await _userManager.GetUserAsync(User);

    if (user == null) {
      return Challenge();
    }

    if (user.IsBlocked) {
      ErrorMessage = "Ваш обліковий запис заблоковано. Видалення улюблених турів недоступне.";
      return RedirectToPage();
    }

    var favorite = await _context.FavoriteTours
        .Include(f => f.Tour)
        .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

    if (favorite == null) {
      ErrorMessage = "Запис улюбленого туру не знайдено.";
      return RedirectToPage();
    }

    _context.FavoriteTours.Remove(favorite);
    await _context.SaveChangesAsync();

    SuccessMessage = "Тур видалено зі списку улюблених.";

    return RedirectToPage();
  }

  private async Task LoadFavoritesAsync() {
    var userId = _userManager.GetUserId(User);

    FavoriteTours = await _context.FavoriteTours
        .Include(f => f.Tour)
            .ThenInclude(t => t!.Country)
        .Include(f => f.Tour)
            .ThenInclude(t => t!.Hotel)
        .Include(f => f.Tour)
            .ThenInclude(t => t!.Reviews)
        .Where(f => f.UserId == userId)
        .OrderByDescending(f => f.AddedAt)
        .ToListAsync();
  }
}