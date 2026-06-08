namespace TourRecommenderPlatform.ViewModels;

public class AdminUserViewModel {
  public string Id { get; set; } = string.Empty;

  public string? FullName { get; set; }

  public string Email { get; set; } = string.Empty;

  public DateTime RegisteredAt { get; set; }

  public bool IsBlocked { get; set; }

  public string Roles { get; set; } = string.Empty;

  public bool IsCurrentUser { get; set; }
}