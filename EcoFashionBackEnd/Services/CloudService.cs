using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcoFashionBackEnd.Exceptions;
using EcoFashionBackEnd.Helpers.Photos;
using Microsoft.Extensions.Options;

namespace EcoFashionBackEnd.Services
{
    public class CloudService
    {
        private readonly Cloudinary _cloudinary;

        public CloudService(IOptions<CloudSettings> cloudSettingsOptions)
        {
            var cloudSettings = cloudSettingsOptions.Value;

            var account = new Account(
                cloudSettings.CloudName,
                cloudSettings.CloudKey,
                cloudSettings.CloudSecret
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0 ||
                (file.ContentType != "image/png" && file.ContentType != "image/jpeg"))
            {
                throw new BadRequestException("File is null, empty, hoặc không đúng định dạng PNG/JPEG.");
            }

            using (var stream = file.OpenReadStream())
            {
                Console.WriteLine($"📤 Uploading file: {file.FileName}, Length: {file.Length}, Type: {file.ContentType}");

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    UploadPreset = "EcoFashion",           
                    //ResourceType = ResourceType.Image 
                };

                try
                {
                    var result = await _cloudinary.UploadAsync(uploadParams);

                    if (result.Error != null)
                    {
                        Console.WriteLine($"❌ Cloudinary Error: {result.Error.Message}");
                        throw new Exception($"Cloudinary upload error: {result.Error.Message}");
                    }

                    Console.WriteLine($"✅ Uploaded: PublicId={result.PublicId}, SecureUrl={result.SecureUrl}");
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Exception during upload: {ex.Message}");
                    throw new Exception("Lỗi khi upload hình lên Cloudinary.", ex);
                }
            }
        }

        public async Task<List<ImageUploadResult>> UploadImagesAsync(List<IFormFile> files)
        {
            var uploadResults = new List<ImageUploadResult>();

            foreach (var file in files)
            {
                var result = await UploadImageAsync(file);
                uploadResults.Add(result);
            }

            return uploadResults;
        }
    }
}
