using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace OHairGanic.BLL.Integrations
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ohairganic/products"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl.ToString();
        }

        public async Task DeleteImageByUrlAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                var parts = imageUrl.Split('/');
                var publicIdWithExt = parts[^1];
                var publicId = Path.GetFileNameWithoutExtension(publicIdWithExt);
                var folder = string.Join("/", parts.SkipWhile(p => p != "ohairganic").Skip(1).TakeWhile(p => !p.Contains(publicIdWithExt)));

                var fullPublicId = $"{folder}/{publicId}";
                await _cloudinary.DestroyAsync(new DeletionParams(fullPublicId));
            }
            catch
            {
                // ignore
            }
        }
    }
}
