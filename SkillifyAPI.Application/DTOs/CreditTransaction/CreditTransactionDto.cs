namespace SkillifyAPI.DTOs.CreditTransaction
{
    public class CreditTransactionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public required string Type { get; set; } // Add / Deduct / Gift / etc
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
