using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Admin.Bookings;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _context;

  public IndexModel(ApplicationDbContext context) {
    _context = context;
  }

  public IList<Booking> Bookings { get; set; } = new List<Booking>();

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task OnGetAsync() {
    await LoadBookingsAsync();
  }

  public async Task<IActionResult> OnPostConfirmAsync(int id) {
    var booking = await _context.Bookings.FindAsync(id);

    if (booking == null) {
      ErrorMessage = "Бронювання не знайдено.";
      return RedirectToPage();
    }

    if (booking.Status == BookingStatuses.Canceled) {
      ErrorMessage = "Скасоване бронювання не можна підтвердити.";
      return RedirectToPage();
    }

    booking.Status = BookingStatuses.Confirmed;
    await _context.SaveChangesAsync();

    SuccessMessage = "Бронювання підтверджено.";

    return RedirectToPage();
  }

  public async Task<IActionResult> OnPostCancelAsync(int id) {
    var booking = await _context.Bookings
        .Include(b => b.Tour)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (booking == null) {
      ErrorMessage = "Бронювання не знайдено.";
      return RedirectToPage();
    }

    if (booking.Status == BookingStatuses.Canceled) {
      ErrorMessage = "Бронювання вже скасовано.";
      return RedirectToPage();
    }

    booking.Status = BookingStatuses.Canceled;

    if (booking.Tour != null) {
      booking.Tour.AvailablePlaces += 1;
    }

    await _context.SaveChangesAsync();

    SuccessMessage = "Бронювання скасовано.";

    return RedirectToPage();
  }

  public async Task<IActionResult> OnPostPendingAsync(int id) {
    var booking = await _context.Bookings.FindAsync(id);

    if (booking == null) {
      ErrorMessage = "Бронювання не знайдено.";
      return RedirectToPage();
    }

    if (booking.Status == BookingStatuses.Canceled) {
      ErrorMessage = "Скасоване бронювання не можна повернути у статус очікування.";
      return RedirectToPage();
    }

    booking.Status = BookingStatuses.Pending;
    await _context.SaveChangesAsync();

    SuccessMessage = "Статус бронювання змінено на очікування.";

    return RedirectToPage();
  }

  private async Task LoadBookingsAsync() {
    Bookings = await _context.Bookings
        .Include(b => b.User)
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Country)
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Hotel)
        .OrderByDescending(b => b.BookingDate)
        .ToListAsync();
  }
}