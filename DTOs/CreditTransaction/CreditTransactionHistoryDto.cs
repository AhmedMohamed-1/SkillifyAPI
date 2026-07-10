using System.Collections.Generic;

namespace SkillifyAPI.DTOs.CreditTransaction
{
    public class CreditTransactionHistoryDto
    {
        public List<CreditTransactionDto> History { get; set; } = new();
        public int CurrentBalance { get; set; }
    }
}
