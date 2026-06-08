using Microsoft.ML;
using Microsoft.ML.Trainers;
using TourRecommenderPlatform.Data;
using TourRecommenderPlatform.Models;
using TourRecommenderPlatform.ViewModels;

namespace TourRecommenderPlatform.Services;

public class RecommendationService {
  private readonly ApplicationDbContext _context;
  private readonly IWebHostEnvironment _environment;

  private const string ModelFileName = "tour-recommendation-model.zip";

  public RecommendationService(
      ApplicationDbContext context,
      IWebHostEnvironment environment) {
    _context = context;
    _environment = environment;
  }

  public async Task<ModelTrainingResultViewModel> TrainModelAsync(string adminUserId) {
    var reviews = _context.Reviews
        .Where(r => r.Rating >= 1 && r.Rating <= 5)
        .Select(r => new RecommendationInput {
          UserId = r.UserId,
          TourId = (uint)r.TourId,
          Label = r.Rating
        })
        .ToList();

    if (reviews.Count < 10) {
      var result = new ModelTrainingResultViewModel {
        IsSuccess = false,
        RecordsCount = reviews.Count,
        Status = TrainingStatuses.NotEnoughData,
        Message = "Недостатньо оцінок для навчання рекомендаційної моделі. Додайте щонайменше 10 оцінок."
      };

      await SaveTrainingLogAsync(result, adminUserId);

      return result;
    }

    try {
      var mlContext = new MLContext(seed: 1);

      IDataView dataView = mlContext.Data.LoadFromEnumerable(reviews);

      var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 1);

      var pipeline = mlContext.Transforms.Conversion.MapValueToKey(
              outputColumnName: "UserIdEncoded",
              inputColumnName: nameof(RecommendationInput.UserId))
          .Append(mlContext.Transforms.Conversion.MapValueToKey(
              outputColumnName: "TourIdEncoded",
              inputColumnName: nameof(RecommendationInput.TourId)))
          .Append(mlContext.Recommendation().Trainers.MatrixFactorization(
              new MatrixFactorizationTrainer.Options {
                MatrixColumnIndexColumnName = "UserIdEncoded",
                MatrixRowIndexColumnName = "TourIdEncoded",
                LabelColumnName = nameof(RecommendationInput.Label),
                NumberOfIterations = 20,
                ApproximationRank = 16
              }));

      var model = pipeline.Fit(split.TrainSet);

      var predictions = model.Transform(split.TestSet);

      var metrics = mlContext.Regression.Evaluate(
          predictions,
          labelColumnName: nameof(RecommendationInput.Label),
          scoreColumnName: "Score");

      string modelDirectory = Path.Combine(_environment.WebRootPath, "ml-models");

      if (!Directory.Exists(modelDirectory)) {
        Directory.CreateDirectory(modelDirectory);
      }

      string modelPath = Path.Combine(modelDirectory, ModelFileName);

      if (File.Exists(modelPath)) {
        File.Delete(modelPath);
      }

      mlContext.Model.Save(model, split.TrainSet.Schema, modelPath);

      var result = new ModelTrainingResultViewModel {
        IsSuccess = true,
        RecordsCount = reviews.Count,
        Rmse = metrics.RootMeanSquaredError,
        Mae = metrics.MeanAbsoluteError,
        Status = TrainingStatuses.Completed,
        ModelPath = $"/ml-models/{ModelFileName}",
        Message = "Рекомендаційну модель успішно навчено та збережено."
      };

      await SaveTrainingLogAsync(result, adminUserId);

      return result;
    } catch (Exception ex) {
      var result = new ModelTrainingResultViewModel {
        IsSuccess = false,
        RecordsCount = reviews.Count,
        Status = TrainingStatuses.Failed,
        Message = $"Помилка під час навчання моделі: {ex.Message}"
      };

      await SaveTrainingLogAsync(result, adminUserId);

      return result;
    }
  }

  public List<(int TourId, float Score)> PredictForUser(string userId, IEnumerable<int> candidateTourIds) {
    string modelPath = Path.Combine(_environment.WebRootPath, "ml-models", ModelFileName);

    if (!File.Exists(modelPath)) {
      return new List<(int TourId, float Score)>();
    }

    var mlContext = new MLContext();

    ITransformer model = mlContext.Model.Load(modelPath, out _);

    var predictionEngine = mlContext.Model.CreatePredictionEngine<RecommendationInput, RecommendationPrediction>(model);

    var predictions = new List<(int TourId, float Score)>();
    foreach (int tourId in candidateTourIds) {
      var input = new RecommendationInput {
        UserId = userId,
        TourId = (uint)tourId,
        Label = 0
      };
      var prediction = predictionEngine.Predict(input);
      predictions.Add((tourId, prediction.Score));
    }
    return predictions
        .OrderByDescending(p => p.Score)
        .ToList();
  }

  public bool ModelExists() {
    string modelPath = Path.Combine(_environment.WebRootPath, "ml-models", ModelFileName);

    return File.Exists(modelPath);
  }

  private async Task SaveTrainingLogAsync(ModelTrainingResultViewModel result, string adminUserId) {
    var log = new ModelTrainingLog {
      TrainingDate = DateTime.Now,
      RecordsCount = result.RecordsCount,
      Rmse = result.Rmse,
      Mae = result.Mae,
      ModelPath = result.ModelPath,
      Status = result.Status,
      UserId = adminUserId,
      Message = result.Message
    };

    _context.ModelTrainingLogs.Add(log);
    await _context.SaveChangesAsync();
  }
}