using SkillifyAPI.DTOs.Gifts;
using SkillifyAPI.Repositories.UserRepository;
using SkillifyAPI.Services.CreditService;

namespace SkillifyAPI.BackgroundService
{
    public class DailyGift
    {
        private readonly IUserRepository _userRepository;
        private readonly ICreditService _creditService;

        public DailyGift(
            IUserRepository userRepository,
            ICreditService creditService)
        {
            _userRepository = userRepository;
            _creditService = creditService;
        }
        public async Task ExecuteAsync()
        {
            var users = await _userRepository.GetEligbleUsersForGift();
            int amount = Random.Shared.Next(5, 101);
            await _creditService.GiveCreditsToUsersAsync(users , amount);
        }
    }
}
