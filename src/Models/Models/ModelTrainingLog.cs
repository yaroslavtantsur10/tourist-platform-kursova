using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourRecommenderPlatform.Models;

public class ModelTrainingLog {
  public int Id { get; set; }

  public DateTime TrainingDate { get; set; } = DateTime.Now;

  public int RecordsCount { get; set; }

  public double? Rmse { get; set; }

  public double? Mae { get; set; }

  [MaxLength(500)]
  public string? ModelPath { get; set; }

  [Required]
  [MaxLength(50)]
  public string Status { get; set; } = TrainingStatuses.Completed;

  [Required]
  public string UserId { get; set; } = string.Empty;

  public ApplicationUser? User { get; set; }

  [MaxLength(2000)]
  public string? Message { get; set; }
}

public static class TrainingStatuses {
  public const string Completed = "Успішно";

  public const string Failed = "Помилка";

  public const string NotEnoughData = "Недостатньо даних";
}