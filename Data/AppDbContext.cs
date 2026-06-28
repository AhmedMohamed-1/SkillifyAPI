using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Models;

namespace SkillifyAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<UserLanguage> UserLanguages => Set<UserLanguage>();
        public DbSet<MainSkill> MainSkills => Set<MainSkill>();
        public DbSet<SubSkill> SubSkills => Set<SubSkill>();
        public DbSet<UserSkill> UserSkills => Set<UserSkill>();
        public DbSet<UserSkillSubSkill> UserSkillSubSkills => Set<UserSkillSubSkill>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<SessionEvent> SessionEvents => Set<SessionEvent>();
        public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();
        public DbSet<EscrowHold> EscrowHolds => Set<EscrowHold>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<Badge> Badges => Set<Badge>();
        public DbSet<UserBadge> UserBadges => Set<UserBadge>();
        public DbSet<PushToken> PushTokens => Set<PushToken>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<UserDevice> UserDevices => Set<UserDevice>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ── User ──────────────────────────────────────────────────────────
            b.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            b.Entity<User>()
                .Property(u => u.CreditBalance)
                .HasDefaultValue(100);

            b.Entity<User>()
                .Property(u => u.ProfileCompleted)
                .HasDefaultValue(false);

            // ── MainSkill & SubSkill catalog ──────────────────────────────────
            b.Entity<MainSkill>()
                .HasIndex(m => m.Slug)
                .IsUnique();

            b.Entity<MainSkill>()
                .HasMany(m => m.SubSkills)
                .WithOne(s => s.MainSkill)
                .HasForeignKey(s => s.MainSkillId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── UserSkill (user ↔ category, offered/needed) ───────────────────
            b.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.Skills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UserSkill>()
                .HasOne(us => us.Category)
                .WithMany()
                .HasForeignKey(us => us.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<UserSkill>()
                .HasIndex(us => new { us.UserId, us.SkillType });

            // ── UserSkillSubSkill (many-to-many join) ─────────────────────────
            b.Entity<UserSkillSubSkill>()
                .HasKey(x => new { x.UserSkillId, x.SubSkillId });

            b.Entity<UserSkillSubSkill>()
                .HasOne(x => x.UserSkill)
                .WithMany(us => us.SubSkills)
                .HasForeignKey(x => x.UserSkillId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UserSkillSubSkill>()
                .HasOne(x => x.SubSkill)
                .WithMany()
                .HasForeignKey(x => x.SubSkillId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Language catalog ──────────────────────────────────────────────
            b.Entity<Language>()
                .HasIndex(l => l.Code)
                .IsUnique();

            // ── UserLanguage ──────────────────────────────────────────────────
            b.Entity<UserLanguage>()
                .HasOne(l => l.User)
                .WithMany(u => u.Languages)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UserLanguage>()
                .HasOne(l => l.Language)
                .WithMany(lang => lang.UserLanguages)
                .HasForeignKey(l => l.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<UserLanguage>()
                .HasIndex(l => new { l.UserId, l.LanguageId })
                .IsUnique();

            // ── Session (requester + helper + main skill) ─────────────────────
            b.Entity<Session>()
                .HasOne(s => s.Requester)
                .WithMany(u => u.RequestedSessions)
                .HasForeignKey(s => s.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Session>()
                .HasOne(s => s.Helper)
                .WithMany(u => u.HelpedSessions)
                .HasForeignKey(s => s.HelperId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Session>()
                .HasOne(s => s.MainSkills)
                .WithMany()
                .HasForeignKey(s => s.MainSkillId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Session>()
                .HasIndex(s => new { s.HelperId, s.Status });

            b.Entity<Session>()
                .HasIndex(s => new { s.RequesterId, s.Status });

            b.Entity<Session>()
                .HasOne(s => s.EscrowHold)
                .WithOne(e => e.Session)
                .HasForeignKey<EscrowHold>(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Session>()
                .HasOne(s => s.Rating)
                .WithOne(r => r.Session)
                .HasForeignKey<Rating>(r => r.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Session>()
                .HasMany(s => s.SessionEvents)
                .WithOne(c => c.Session)
                .HasForeignKey(c => c.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Session>()
                .HasMany(s => s.CreditTransactions)
                .WithOne(t => t.Session)
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── SessionEvent ───────────────────────────────────────────
            b.Entity<SessionEvent>()
                .HasOne(e => e.User)
                .WithMany(u => u.SessionEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<SessionEvent>()
                .HasOne(e => e.Session)
                .WithMany(s => s.SessionEvents)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<SessionEvent>()
                .HasIndex(e => new { e.SessionId, e.CreatedAt });

            b.Entity<SessionEvent>()
                .HasIndex(e => e.Type);

            // ── EscrowHold (one per session) ──────────────────────────────────
            b.Entity<EscrowHold>()
                .HasIndex(e => e.SessionId)
                .IsUnique();

            b.Entity<EscrowHold>()
                .HasOne(e => e.Requester)
                .WithMany(u => u.EscrowHolds)
                .HasForeignKey(e => e.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Rating (one per session; reviewer + reviewee) ─────────────────
            b.Entity<Rating>()
                .HasIndex(r => r.SessionId)
                .IsUnique();

            b.Entity<Rating>()
                .Property(r => r.Score)
                .HasPrecision(3, 1);

            b.Entity<Rating>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.GivenRatings)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Rating>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReceivedRatings)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Rating>()
                .HasIndex(r => r.RevieweeId);

            // ── CreditTransaction (append-only ledger) ────────────────────────
            b.Entity<CreditTransaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.CreditTransactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<CreditTransaction>()
                .HasIndex(t => new { t.UserId, t.CreatedAt });

            // ── Badge & UserBadge ─────────────────────────────────────────────
            b.Entity<Badge>()
                .HasIndex(bg => bg.Slug)
                .IsUnique();

            b.Entity<UserBadge>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.Badges)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UserBadge>()
                .HasOne(ub => ub.Badge)
                .WithMany(bg => bg.UserBadges)
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<UserBadge>()
                .HasIndex(ub => new { ub.UserId, ub.BadgeId })
                .IsUnique();

            // ── PushToken ─────────────────────────────────────────────────────
            b.Entity<PushToken>()
                .HasOne(p => p.User)
                .WithMany(u => u.PushTokens)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<PushToken>()
                .HasIndex(p => p.Token)
                .IsUnique();

            // ── Refresh Token relations ─────────────────────────────────────────────────────
            b.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);

            b.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            b.Entity<RefreshToken>()
                .Property(rt => rt.Id)
                .UseIdentityColumn();

            // ── Notification ───────────────────────────────────────────────────
            b.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.CreatedAt });

            // ── UserDevice ─────────────────────────────────────────────────────
            b.Entity<UserDevice>()
                .HasOne(d => d.User)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UserDevice>()
                .HasIndex(d => d.FcmToken)
                .IsUnique();

            b.Entity<UserDevice>()
                .HasIndex(d => new { d.UserId, d.IsActive });
        }
    }
}
