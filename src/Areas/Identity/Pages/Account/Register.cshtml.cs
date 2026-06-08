using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel {
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IUserStore<ApplicationUser> _userStore;
  private readonly IUserEmailStore<ApplicationUser> _emailStore;
  private readonly ILogger<RegisterModel> _logger;
  private readonly IEmailSender _emailSender;

  public RegisterModel(
      UserManager<ApplicationUser> userManager,
      IUserStore<ApplicationUser> userStore,
      SignInManager<ApplicationUser> signInManager,
      ILogger<RegisterModel> logger,
      IEmailSender emailSender) {
    _userManager = userManager;
    _userStore = userStore;
    _emailStore = GetEmailStore();
    _signInManager = signInManager;
    _logger = logger;
    _emailSender = emailSender;
  }

  [BindProperty]
  public InputModel Input { get; set; } = new();

  public string ReturnUrl { get; set; } = string.Empty;

  public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

  public class InputModel {
    [Required(ErrorMessage = "Вкажіть повне ім’я")]
    [Display(Name = "Повне ім’я")]
    [StringLength(150, ErrorMessage = "Повне ім’я не може перевищувати 150 символів")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть електронну пошту")]
    [EmailAddress(ErrorMessage = "Вкажіть коректну електронну пошту")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть пароль")]
    [StringLength(100, ErrorMessage = "Пароль має містити щонайменше {2} символів.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження пароля")]
    [Compare("Password", ErrorMessage = "Пароль і підтвердження пароля не збігаються.")]
    public string ConfirmPassword { get; set; } = string.Empty;
  }

  public async Task OnGetAsync(string? returnUrl = null) {
    ReturnUrl = returnUrl ?? Url.Content("~/");
    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
  }

  public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
    returnUrl ??= Url.Content("~/");
    ReturnUrl = returnUrl;

    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

    if (!ModelState.IsValid) {
      return Page();
    }

    var user = CreateUser();

    user.FullName = Input.FullName;
    user.RegisteredAt = DateTime.Now;
    user.IsBlocked = false;

    await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
    await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

    var result = await _userManager.CreateAsync(user, Input.Password);

    if (result.Succeeded) {
      _logger.LogInformation("Новий користувач створив обліковий запис.");

      if (await _userManager.IsInRoleAsync(user, RoleNames.User) == false) {
        await _userManager.AddToRoleAsync(user, RoleNames.User);
      }

      var userId = await _userManager.GetUserIdAsync(user);
      var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

      var callbackUrl = Url.Page(
          "/Account/ConfirmEmail",
          pageHandler: null,
          values: new { area = "Identity", userId, code, returnUrl },
          protocol: Request.Scheme);

      await _emailSender.SendEmailAsync(
          Input.Email,
          "Підтвердження електронної пошти",
          $"Підтвердіть обліковий запис, перейшовши за посиланням: <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>підтвердити</a>.");

      await _signInManager.SignInAsync(user, isPersistent: false);

      return LocalRedirect(returnUrl);
    }

    foreach (var error in result.Errors) {
      ModelState.AddModelError(string.Empty, error.Description);
    }

    return Page();
  }

  private ApplicationUser CreateUser() {
    try {
      return Activator.CreateInstance<ApplicationUser>();
    } catch {
      throw new InvalidOperationException($"Не вдалося створити екземпляр '{nameof(ApplicationUser)}'.");
    }
  }

  private IUserEmailStore<ApplicationUser> GetEmailStore() {
    if (!_userManager.SupportsUserEmail) {
      throw new NotSupportedException("Для реєстрації потрібна підтримка електронної пошти.");
    }

    return (IUserEmailStore<ApplicationUser>)_userStore;
  }
}