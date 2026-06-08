namespace TourRecommenderPlatform.ViewModels;

public class ModelTrainingResultViewModel {
  public bool IsSuccess { get; set; }

  public int RecordsCount { get; set; }

  public double? Rmse { get; set; }

  public double? Mae { get; set; }

  public string Status { get; set; } = string.Empty;

  public string Message { get; set; } = string.Empty;

  public string? ModelPath { get; set; }
}