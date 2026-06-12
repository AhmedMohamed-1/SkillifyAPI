using SkillifyAPI.DTOs.Badge.BadgeDTO;
using SkillifyAPI.DTOs.Language;
using SkillifyAPI.DTOs.Language.LanguageDTO;
using SkillifyAPI.DTOs.Rating;
using SkillifyAPI.DTOs.Skill.SkillDTO;
using SkillifyAPI.DTOs.User.UserDTO;
using SkillifyAPI.Models;

namespace SkillifyAPI.Helper
{
    public static class UserProfileMapper
    {
        public static GetUserSkillDTO? ToUserSkillDto(UserSkill? userSkill)
        {
            if (userSkill == null)
                return null;

            return new GetUserSkillDTO
            {
                UserSkillId = userSkill.Id,
                Description = userSkill.Description,
                MainSkill = new GetMainSkillDTO
                {
                    Id = userSkill.Category.Id,
                    Name = userSkill.Category.Name,
                    Slug = userSkill.Category.Slug,
                    IconKey = userSkill.Category.IconKey
                },
                SubSkills = userSkill.SubSkills.Select(x => new GetSubSkillDTO
                {
                    Id = x.SubSkill.Id,
                    Name = x.SubSkill.Name,
                    IconKey = x.SubSkill.IconKey
                }).ToList()
            };
        }

        public static GetUserProfileData ToProfileDto(User user, int completedSessionsCount)
        {
            var offered = user.Skills.FirstOrDefault(s => s.SkillType == SkillType.Offered);
            var needed = user.Skills.FirstOrDefault(s => s.SkillType == SkillType.Needed);
            var reviews = user.ReceivedRatings.ToList();

            return new GetUserProfileData
            {
                UserID = user.Id,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                JobTitle = user.JobTitle,
                CreditBalance = user.CreditBalance,
                Badges = user.Badges.Select(ub => new GetBadgeDTO
                {
                    Id = ub.Badge.Id,
                    Name = ub.Badge.Name,
                    Slug = ub.Badge.Slug,
                    Description = ub.Badge.Description,
                    IconKey = ub.Badge.IconKey
                }).ToList(),
                Languages = user.Languages.Select(ul => new GetLanguageDTO
                {
                    Id = ul.Language.Id,
                    Name = ul.Language.Name,
                    Code = ul.Language.Code
                }).ToList(),
                OfferedSkill = ToUserSkillDto(offered),
                NeededSkill = ToUserSkillDto(needed),
                CompletedSessions = completedSessionsCount.ToString(),
                ReceivedReviews = reviews.Select(r => new GetReceivedReviewDTO
                {
                    Id = r.Id,
                    SessionId = r.SessionId,
                    Score = r.Score,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt,
                    Reviewer = new GetReviewerSummaryDTO
                    {
                        UserId = r.Reviewer.Id,
                        FullName = r.Reviewer.FullName,
                        ProfilePictureUrl = r.Reviewer.ProfilePictureUrl
                    }
                }).ToList(),
                OverallRatingScore = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Score), 1)
                    : null
            };
        }

        public static UsersListDTO ToListItemDto(User user)
        {
            var offered = user.Skills.FirstOrDefault(s => s.SkillType == SkillType.Offered);
            var needed = user.Skills.FirstOrDefault(s => s.SkillType == SkillType.Needed);

            return new UsersListDTO
            {
                UserId = user.Id,
                FullName = user.FullName,
                JobTitle = user.JobTitle,
                ProfilePictureUrl = user.ProfilePictureUrl,
                OfferedMainSkill = offered?.Category == null ? null : new GetMainSkillDTO
                {
                    Id = offered.Category.Id,
                    Name = offered.Category.Name,
                    Slug = offered.Category.Slug,
                    IconKey = offered.Category.IconKey
                },
                NeededMainSkill = needed?.Category == null ? null : new GetMainSkillDTO
                {
                    Id = needed.Category.Id,
                    Name = needed.Category.Name,
                    Slug = needed.Category.Slug,
                    IconKey = needed.Category.IconKey
                }
            };
        }
    }
}
