namespace LaptopStore.Helpers;

public static class FileUploadHelper
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"];

    public static async Task<string?> SaveAsync(IFormFile? file, string webRoot, string folder)
    {
        if (file == null || file.Length == 0)
            return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            throw new InvalidOperationException("Định dạng ảnh không được hỗ trợ.");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Ảnh không được vượt quá 5MB.");

        var dir = Path.Combine(webRoot, folder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return "/" + folder.Trim('/') + "/" + name;
    }
}
