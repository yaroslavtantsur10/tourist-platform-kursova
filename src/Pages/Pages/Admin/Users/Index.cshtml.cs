using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Pages.Admin.Users;

[Authorize(Roles = RoleNames.Administrator)]
public class IndexModel : PageModel {
  private readonly UserManager<ApplicationUser> _userManager;

  public IndexModel(UserManager<ApplicationUser> userManager) {
    _userManager = userManager;
  }

  public List<AdminUserViewModel> UsersList { get; set; } = new();

  [TempData]
  public string? SuccessMessage { get; set; }

  [TempData]
  public string? ErrorMessage { get; set; }

  public async Task OnGetAsync() {
    await LoadUsersAsync();
  }

  public async Task<IActionResult> OnPostBlockAsync(string id) {
    var currentUserId = _userManager.GetUserId(User);

    if (id == currentUserId) {
      ErrorMessage = "Адміністратор не може заблокувати власний обліковий запис.";
      return RedirectToPage();
    }

    var user = await _userManager.FindByIdAsync(id);

    if (user == null) {
      ErrorMessage = "Користувача не знайдено.";
      return RedirectToPage();
    }

    if (user.IsBlocked) {
      ErrorMessage = "Користувач уже заблокований.";
      return RedirectToPage();
    }

    user.IsBlocked = true;

    var result = await _userManager.UpdateAsync(user);

    if (result.Succeeded) {
      SuccessMessage = $"Користувача {user.Email} заблоковано.";
    } else {
      ErrorMessage = "Не вдалося заблокувати користувача.";
    }

    return RedirectToPage();
  }

  public async Task<IActionResult> OnPostUnblockAsync(string id) {
    var user = await _userManager.FindByIdAsync(id);

    if (user == null) {
      ErrorMessage = "Користувача не знайдено.";
      return RedirectToPage();
    }

    if (!user.IsBlocked) {
      ErrorMessage = "Користувач уже активний.";
      return RedirectToPage();
    }

    user.IsBlocked = false;

    var result = await _userManager.UpdateAsync(user);

    if (result.Succeeded) {
      SuccessMessage = $"Користувача {user.Email} розблоковано.";
    } else {
      ErrorMessage = "Не вдалося розблокувати користувача.";
    }

    return RedirectToPage();
  }

  private async Task LoadUsersAsync() {
    var currentUserId = _userManager.GetUserId(User);

    var users = await _userManager.Users
        .OrderByDescending(u => u.RegisteredAt)
        .ToListAsync();

    UsersList = new List<AdminUserViewModel>();

    foreach (var user in users) {
      var roles = await _userManager.GetRolesAsync(user);

      UsersList.Add(new AdminUserViewModel {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email ?? string.Empty,
        RegisteredAt = user.RegisteredAt,
        IsBlocked = user.IsBlocked,
        Roles = roles.Any() ? string.Join(", ", roles) : "Без ролі",
        IsCurrentUser = user.Id == currentUserId
      });
    }
  }
}