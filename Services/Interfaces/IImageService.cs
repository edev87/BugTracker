using BugTracker.Enums;

namespace BugTracker.Services.Interfaces
{
    public interface IImageService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file);

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage);
      //  Task<byte[]?> ConvertFileToByteArrayAsync(IFormFile imageFile);
    }
}
