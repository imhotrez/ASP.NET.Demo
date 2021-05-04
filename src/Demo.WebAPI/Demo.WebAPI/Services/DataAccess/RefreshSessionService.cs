using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Models.Domain.Auth;
using Demo.Models.Filters;
using Microsoft.EntityFrameworkCore;

namespace Demo.WebAPI.Services.DataAccess {
    public class RefreshSessionService : BaseDbService<RefreshSession, RefreshSession, BaseFilter> {
        public RefreshSessionService(DemoContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public async Task<RefreshSession> GetSessionByGuid(string refreshToken, CancellationToken cancellationToken) {
            if (!Guid.TryParse(refreshToken, out var guid)) {
                throw new Exception("Не распознан рефреш-токен");
            }

            var session = await DbSet.FirstOrDefaultAsync(p => p.RefreshToken == guid, cancellationToken);

            return session;
        }

        public async Task<RefreshSession> TotalLogout(string refreshToken, CancellationToken cancellationToken) {
            if (!Guid.TryParse(refreshToken, out var guid)) {
                throw new Exception("Не распознан рефреш-токен");
            }

            var session = await DbSet.FirstOrDefaultAsync(p => p.RefreshToken == guid, cancellationToken);

            var anotherSessions = DbSet.Where(x => x.UserId == session.UserId && x.RefreshToken != guid);
            DbSet.RemoveRange(anotherSessions);

            return session;
        }

        public override async Task<RefreshSession> Save(RefreshSession dto,
            CancellationToken cancellationToken = default) {
            var propsNames = new List<string> {
                nameof(dto.UserId),
                nameof(dto.RefreshToken),
                nameof(dto.UserAgent),
                nameof(dto.Fingerprint),
                nameof(dto.IPAddress),
                nameof(dto.ExpiresIn),
            };

            return await Save(dto, propsNames, cancellationToken);
        }
    }
}