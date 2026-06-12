using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SkillifyAPI.CloudinaryService
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _defaultFolder;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };

            _defaultFolder = settings.CloudFolder;
        }

        // ── Upload ───────────────────────────────────────────
        public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string? folder = null)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            var targetFolder = folder ?? _defaultFolder;

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = targetFolder,
                // Auto-generate a unique public ID
                PublicId = $"{targetFolder}/{Guid.NewGuid()}",
                // Auto-optimize quality & format
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto"),
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
                throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

            return new CloudinaryUploadResult
            {
                SecureUrl = result.SecureUrl.ToString(),
                PublicId = result.PublicId
            };
        }

        // ── Delete ───────────────────────────────────────────
        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}

