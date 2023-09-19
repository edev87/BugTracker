using BugTracker.Enums;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services
{
    public class ImageService : IImageService
    {
        private readonly string? _defaultImage = "/img/DefaultContactImage.png";
        private readonly string? _defaultBlogImage = "/img/DefaultBlogImage.png";
        private readonly string? _blogAuthorImage = "/img/DefaultBlogImage.png";
        private readonly string? _defaultUserImage = "/img/DefaultBlogImage.png";
        private readonly string? _defaultCategoryImage = "/img/DefaultBlogImage.png";
        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
        {
            try
            {
                if (fileData == null || fileData.Length == 0)
                {
                    switch (defaultImage)
                    {
                        case DefaultImage.BTUserImage: return _blogAuthorImage;
                        case DefaultImage.ProjectImage: return _defaultBlogImage;
                        case DefaultImage.CompanyImage: return _defaultUserImage;
                    }
                }
                string? imageBase64Data = Convert.ToBase64String(fileData!);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageBase64Data;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file!.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }

       
    }
}
