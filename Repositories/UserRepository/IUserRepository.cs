using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.UserRepository
{
    /// <summary>
    /// Repository for User entity.
    /// Note: This repository implements the Unit of Work pattern. 
    /// Write operations (SignUpAsync, AddUserSkillAsync, AddRefreshTokenAsync, UpdateRefreshToken, RemoveUserSkillsAsync, RevokeAllForUserAsync) 
    /// do not automatically save changes to the database. 
    /// You must explicitly call <see cref="SaveChangesAsync(CancellationToken)"/> to commit transactions.
    /// </summary>
    public interface IUserRepository
    {
        Task<bool> SignUpAsync(User user, CancellationToken ct = default);
        Task<User?> GetUserByIdAsync(int userId, CancellationToken ct = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetUserForProfileAsync(int userId, CancellationToken ct = default);
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<int> GetCompletedSessionsCountAsync(int userId, CancellationToken ct = default);
        Task<bool> MainSkillExistsAsync(int mainSkillId, CancellationToken ct = default);
        Task<bool> SubSkillsExistForMainSkillAsync(int mainSkillId, IEnumerable<int> subSkillIds, CancellationToken ct = default);
        Task RemoveUserSkillsAsync(int userId, CancellationToken ct = default);
        Task AddUserSkillAsync(UserSkill userSkill, CancellationToken ct = default);
        
        /// <summary>
        /// Commits all pending changes to the database. Must be called after write operations.
        /// </summary>
        Task SaveChangesAsync(CancellationToken ct = default);

        Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
        Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default);
        Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensForUserAsync(int userId, CancellationToken ct = default);
        void UpdateRefreshToken(RefreshToken token);
        Task RevokeAllForUserAsync(int userId, string reason, CancellationToken ct = default);
    }
}
