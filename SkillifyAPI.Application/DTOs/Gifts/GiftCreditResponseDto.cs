namespace SkillifyAPI.DTOs.Gifts
{
    public class GiftCreditResponseDto
    {
        public int UserId { get; set; }

        public int GiftAmount { get; set; }

        public int NewBalance { get; set; }

        public DateTime GrantedAt { get; set; }
    }
}
