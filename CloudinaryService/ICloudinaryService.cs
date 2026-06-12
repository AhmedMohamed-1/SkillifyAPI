namespace SkillifyAPI.CloudinaryService
{
    public interface ICloudinaryService
    {
        /// <summary>Upload any image and return its secure URL and public ID.</summary>
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string? folder = null);

        /// <summary>Delete an image by its public ID.</summary>
        Task<bool> DeleteImageAsync(string publicId);
    }
}
