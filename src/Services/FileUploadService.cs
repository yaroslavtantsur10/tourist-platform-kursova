namespace TourRecommenderPlatform.Services;

public class FileUploadService {
  private readonly IWebHostEnvironment _environment;

  public FileUploadService(IWebHostEnvironment environment) {
    _environment = environment;
  }

  public async Task<string?> UploadTourPhotoAsync(IFormFile? file) {
    if (file == null || file.Length == 0) {
      return null;
    }

    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

    if (!allowedExtensions.Contains(extension)) {
      throw new InvalidOperationException("Дозволено завантажувати лише файли JPG, JPEG, PNG або WEBP.");
    }

    const long maxFileSize = 5 * 1024 * 1024;

    if (file.Length > maxFileSize) {
      throw new InvalidOperationException("Розмір фото не повинен перевищувати 5 МБ.");
    }

    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "tours");

    if (!Directory.Exists(uploadsFolder)) {
      Directory.CreateDirectory(uploadsFolder);
    }

    string fileName = $"{Guid.NewGuid()}{extension}";
    string filePath = Path.Combine(uploadsFolder, fileName);

    using FileStream stream = new(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    return $"/uploads/tours/{fileName}";
  }

  public void DeletePhotoIfExists(string? photoPath) {
    if (string.IsNullOrWhiteSpace(photoPath)) {
      return;
    }

    if (photoPath.Contains("default-tour.jpg")) {
      return;
    }

    string relativePath = photoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
    string fullPath = Path.Combine(_environment.WebRootPath, relativePath);

    if (File.Exists(fullPath)) {
      File.Delete(fullPath);
    }
  }
}