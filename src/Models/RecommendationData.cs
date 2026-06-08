using Microsoft.ML.Data;

namespace TourRecommenderPlatform.Models;

public class RecommendationInput {
  public string UserId { get; set; } = string.Empty;

  public uint TourId { get; set; }

  public float Label { get; set; }
}

public class RecommendationPrediction {
  public float Score { get; set; }
}