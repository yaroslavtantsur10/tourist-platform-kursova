using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Pages.Bookings;

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

  public IList<Booking> Bookings { get; set; } = new List<Booking>();

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task OnGetAsync() {
    await LoadBookingsAsync();
  }

  public async Task<IActionResult> OnPostCreateAsync(int tourId) {
    var user = await _userManager.GetUserAsync(User);

    if (user == null) {
      return Challenge();
    }

    if (user.IsBlocked) {
      ErrorMessage = "Ваш обліковий запис заблоковано. Створення бронювань недоступне.";
      return RedirectToPage("/Tours/Details", new { id = tourId });
    }

    var tour = await _context.Tours
        .FirstOrDefaultAsync(t => t.Id == tourId && t.IsActive);

    if (tour == null) {
      ErrorMessage = "Тур не знайдено або він більше не є активним.";
      return RedirectToPage("/Tours/Index");
    }

    if (tour.AvailablePlaces <= 0) {
      ErrorMessage = "Неможливо створити бронювання, оскільки доступних місць немає.";
      return RedirectToPage("/Tours/Details", new { id = tourId });
    }

    bool alreadyHasActiveBooking = await _context.Bookings
        .AnyAsync(b =>
            b.UserId == user.Id &&
            b.TourId == tourId &&
            b.Status != BookingStatuses.Canceled);

    if (alreadyHasActiveBooking) {
      ErrorMessage = "Ви вже маєте активне бронювання для цього туру.";
      return RedirectToPage("/Tours/Details", new { id = tourId });
    }

    var booking = new Booking {
      UserId = user.Id,
      TourId = tour.Id,
      BookingDate = DateTime.Now,
      Status = BookingStatuses.Pending
    };

    tour.AvailablePlaces -= 1;

    _context.Bookings.Add(booking);
    await _context.SaveChangesAsync();

    SuccessMessage = "Бронювання успішно створено. Статус бронювання: очікує.";

    return RedirectToPage();
  }

  public async Task<IActionResult> OnPostCancelAsync(int id) {
    var user = await _userManager.GetUserAsync(User);

    if (user == null) {
      return Challenge();
    }

    if (user.IsBlocked) {
      ErrorMessage = "Ваш обліковий запис заблоковано. Скасування бронювання недоступне.";
      return RedirectToPage();
    }

    var booking = await _context.Bookings
        .Include(b => b.Tour)
        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

    if (booking == null) {
      ErrorMessage = "Бронювання не знайдено.";
      return RedirectToPage();
    }

    if (booking.Status == BookingStatuses.Canceled) {
      ErrorMessage = "Це бронювання вже скасовано.";
      return RedirectToPage();
    }

    booking.Status = BookingStatuses.Canceled;

    if (booking.Tour != null) {
      booking.Tour.AvailablePlaces += 1;
    }

    await _context.SaveChangesAsync();

    SuccessMessage = "Бронювання успішно скасовано.";

    return RedirectToPage();
  }

  private async Task LoadBookingsAsync() {
    var userId = _userManager.GetUserId(User);

    Bookings = await _context.Bookings
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Country)
        .Include(b => b.Tour)
            .ThenInclude(t => t!.Hotel)
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.BookingDate)
        .ToListAsync();
  }
}