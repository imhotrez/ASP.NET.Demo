using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Demo.Models.Domain.Auth;
using Demo.Models.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demo.DAL {
    public sealed class DemoContext : IdentityDbContext<AppUser, AppRole, long, AppUserClaim, AppUserRole, AppUserLogin,
        AppRoleClaim, AppUserToken> {
        public DemoContext(DbContextOptions options) : base(options) {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            Database.SetCommandTimeout(10000);
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>(b => {
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
                b.HasMany(e => e.RefreshSessions)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired();
                b.ToTable(nameof(AppUser) + 's', "auth");
            });

            builder.Entity<AppRole>(b => {
                
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
                b.ToTable(nameof(AppRole) + 's', "auth");
            });


            builder.Entity<AppRoleClaim>(b => b.ToTable(nameof(AppRoleClaim) + 's', "auth"));
            builder.Entity<AppUserClaim>(b => b.ToTable(nameof(AppUserClaim) + 's', "auth"));
            builder.Entity<AppUserLogin>(b => b.ToTable(nameof(AppUserLogin) + 's', "auth"));
            builder.Entity<AppUserRole>(b => b.ToTable(nameof(AppUserRole) + 's', "auth"));
            builder.Entity<AppUserToken>(b => b.ToTable(nameof(AppUserToken) + 's', "auth"));
            builder.Entity<RefreshSession>(b => b.ToTable(nameof(RefreshSession) + 's', "auth"));
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            var entities = ChangeTracker
                .Entries()
                .Where(p => p.Entity.GetType().GetTypeInfo().GetInterfaces().Contains(typeof(IDatedEntity)))
                .ToArray();

            var modified = entities.Where(p => p.State == EntityState.Modified).Select(p => p.Entity);

            foreach (var item in modified) {
                Entry(item).Property(nameof(IDatedEntity.UpdateDate)).CurrentValue = System.DateTime.UtcNow;
                Entry(item).Property(nameof(IDatedEntity.CreateDate)).IsModified = false;
            }

            var added = entities.Where(p => p.State == EntityState.Added).Select(p => p.Entity);

            foreach (var item in added) {
                Entry(item).Property(nameof(IDatedEntity.CreateDate)).CurrentValue = System.DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}