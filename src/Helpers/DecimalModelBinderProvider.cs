using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TourRecommenderPlatform.Infrastructure.ModelBinders;

public class DecimalModelBinderProvider : IModelBinderProvider {
  public IModelBinder? GetBinder(ModelBinderProviderContext context) {
    if (context == null) {
      throw new ArgumentNullException(nameof(context));
    }

    var modelType = context.Metadata.UnderlyingOrModelType;

    if (modelType == typeof(decimal)) {
      return new DecimalModelBinder();
    }

    return null;
  }
}