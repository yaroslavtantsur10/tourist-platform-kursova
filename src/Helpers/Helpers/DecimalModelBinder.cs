using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace TourRecommenderPlatform.Infrastructure.ModelBinders;

public class DecimalModelBinder : IModelBinder {
  public Task BindModelAsync(ModelBindingContext bindingContext) {
    if (bindingContext == null) {
      throw new ArgumentNullException(nameof(bindingContext));
    }

    var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

    if (valueProviderResult == ValueProviderResult.None) {
      return Task.CompletedTask;
    }

    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

    var value = valueProviderResult.FirstValue;

    if (string.IsNullOrWhiteSpace(value)) {
      return Task.CompletedTask;
    }

    value = value.Trim();

    var normalizedValue = value.Replace(',', '.');

    if (decimal.TryParse(
        normalizedValue,
        NumberStyles.Number,
        CultureInfo.InvariantCulture,
        out var result)) {
      bindingContext.Result = ModelBindingResult.Success(result);
    } else {
      bindingContext.ModelState.TryAddModelError(
        bindingContext.ModelName,
        "Введіть коректне числове значення.");
    }

    return Task.CompletedTask;
  }
}