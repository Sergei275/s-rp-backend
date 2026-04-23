using SRp.Data;
using SRp.Models;
using Microsoft.EntityFrameworkCore;

namespace SRp.Services
{
    public class SessionService
    {
        private readonly SessionContext _sessionContext;

        private const long IdleTimeoutSeconds = 60 * 60;
        private const long TouchTimeoutSeconds = 15 * 60;
        private const long SessionexpiresDays = 15;

        public SessionService(SessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        public async Task<Session> CreateSession(Player player)
        {
            var session = await FindActiveSessionBySteamId(player.SteamId64);

            if (session != null)
                return session;

            Session newSession = new();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            newSession.OwnerSteamId64 = player.SteamId64;
            newSession.SessionId = Guid.NewGuid();
            newSession.CreatedAt = now;
            newSession.ExpiresAt = DateTimeOffset.UtcNow.AddDays(SessionexpiresDays).ToUnixTimeSeconds();
            newSession.LastSeenAt = now;

            _sessionContext.Sessions.Add(newSession);
            await _sessionContext.SaveChangesAsync();

            return newSession;
        }

        public async Task CloseSession(Guid sessionId)
        {
            var session = await FindActiveSessionById(sessionId);

            if (session == null)
                return;

            session.RevokedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _sessionContext.SaveChangesAsync();
        }

        public async Task<bool> TouchSession(Session tuchedSession)
        {
            if (tuchedSession == null)
                return false;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var isSessionAlive = now - tuchedSession.LastSeenAt >= TouchTimeoutSeconds;

            if (!isSessionAlive)
                return true;

            tuchedSession.LastSeenAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _sessionContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsSessionExpires(Session chekedSession)
        {
            if (chekedSession == null)
                return true;

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (chekedSession.ExpiresAt < currentTime)
                return true;

            return false;
        }

        public async Task<bool> IsSessionClosed(Session chekedSession)
        {
            if (chekedSession == null)
                return true;

            if (chekedSession.RevokedAt != null)
                return true;

            return false;
        }

        public async Task<Session?> FindLatestSession(Guid sessionId)
        {
            var session = await _sessionContext.Sessions
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(p => p.SessionId == sessionId);

            if (session == null)
                return null;

            return session;
        }

        public async Task<Session?> FindActiveSessionById(Guid sessionId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var session = await _sessionContext.Sessions
                .FirstOrDefaultAsync(p => 
                p.SessionId == sessionId
                && p.RevokedAt == null
                && p.ExpiresAt > now
                && p.LastSeenAt > now - IdleTimeoutSeconds);

            if (session == null)
                return null;

            return session;
        }

        public async Task<Session?> FindActiveSessionBySteamId(long steamId64)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var session = await _sessionContext.Sessions
                .FirstOrDefaultAsync(p => p.OwnerSteamId64 == steamId64
                && p.RevokedAt == null
                && p.ExpiresAt > now
                && p.LastSeenAt > now - IdleTimeoutSeconds);

            if (session == null)
                return null;

            return session;
        }
    }
}
