namespace SkillifyAPI.DTOs.Gifts
{
    public class BulkGiftCreditsDto
    {
        public List<int> UserIds { get; set; } = [];

        public int Amount { get; set; }
    }
}
