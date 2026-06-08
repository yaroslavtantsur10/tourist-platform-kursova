using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel {
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<LoginModel> _logger;

  public LoginModel(
      SignInManager<ApplicationUser> signInManager,
      UserManager<ApplicationUser> userManager,
      ILogger<LoginModel> logger) {
    _signInManager = signInManager;
    _userManager = userManager;
    _logger = logger;
  }

  [BindProperty]
  public InputModel Input { get; set; } = new();

  public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

  public string ReturnUrl { get; set; } = string.Empty;

  [TempData]
  public string? ErrorMessage { get; set; }

  public class InputModel {
    [Required(ErrorMessage = "Вкажіть електронну пошту")]
    [EmailAddress(ErrorMessage = "Вкажіть коректну електронну пошту")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запам’ятати мене")]
    public bool RememberMe { get; set; }
  }

  public async Task OnGetAsync(string? returnUrl = null) {
    if (!string.IsNullOrEmpty(ErrorMessage)) {
      ModelState.AddModelError(string.Empty, ErrorMessage);
    }

    returnUrl ??= Url.Content("~/");

    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

    ReturnUrl = returnUrl;
  }

  public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
    returnUrl ??= Url.Content("~/");

    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

    if (!ModelState.IsValid) {
      return Page();
    }

    var user = await _userManager.FindByEmailAsync(Input.Email);

    if (user == null) {
      ModelState.AddModelError(string.Empty, "Невдала спроба входу.");
      return Page();
    }

    if (user.IsBlocked) {
      ModelState.AddModelError(string.Empty, "Ваш обліковий запис заблоковано адміністратором.");
      return Page();
    }

    var result = await _signInManager.PasswordSignInAsync(
        user.UserName ?? Input.Email,
        Input.Password,
        Input.RememberMe,
        lockoutOnFailure: false);

    if (result.Succeeded) {
      _logger.LogInformation("Користувач увійшов до системи.");
      return LocalRedirect(returnUrl);
    }

    if (result.IsLockedOut) {
      _logger.LogWarning("Обліковий запис користувача заблоковано механізмом Identity.");
      return RedirectToPage("./Lockout");
    }

    ModelState.AddModelError(string.Empty, "Невдала спроба входу.");
    return Page();
  }
}