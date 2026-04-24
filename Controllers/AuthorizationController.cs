using SRp.Data;
using SRp.Models;
using SRp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SRp.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly PlayerInventoryContext _playerInventoryContext;
        private readonly TokenService _tokenService;
        private readonly AuthorizationContext _authcontext;
        private readonly RefreshTokenContext _refreshTokenContext;
        private readonly FacePunchAuthService _facePunchAuthService;

        private SessionService _sessionService;

        public AuthorizationController(
            PlayerInventoryContext playerInventoryContext,
            TokenService tokenService,
            AuthorizationContext context,
            RefreshTokenContext refreshTokenContext,
            SessionService sessionService,
            FacePunchAuthService facePunchAuthService)
        {
            _playerInventoryContext = playerInventoryContext;
            _tokenService = tokenService;
            _authcontext = context;
            _refreshTokenContext = refreshTokenContext;
            _sessionService = sessionService;
            _facePunchAuthService = facePunchAuthService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthTokensResponse>> Login([FromBody] LoginRequest req)
        {
            if (req == null)
                return BadRequest();

            var isTokenValid = await _facePunchAuthService.ValidateToken(req);

            if (!isTokenValid)
                return Conflict(new { isTokenValid = false });

            var player = await GetPlayer(req.SteamId64);

            if (player == null)
                player = await CreatePlayer(req.SteamId64);

            if (player == null)
                return Conflict(new { created = false });

            var session = await _sessionService.CreateSession(player);

            var (refresh, refreshExp) = _tokenService.CreateRefreshToken();
            var refreshHash = TokenService.Sha256(refresh);

            RefreshToken refreshToken = new()
            {
                SessionId = session.SessionId,
                TokenHash = refreshHash,
                ExpiresAtUtc = refreshExp,
            };

            var (access, expiresIn) = _tokenService.CreateAccessToken(req.SteamId64, session.SessionId);

            _refreshTokenContext.RefreshTokens.Add(refreshToken);
            player.LastSeenAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _refreshTokenContext.SaveChangesAsync();
            await _authcontext.SaveChangesAsync();

            AuthTokensResponse response = new()
            {
                AccessToken = access,
                ExpiresInSeconds = expiresIn,
                RefreshToken = refresh,
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthTokensResponse>> Refresh([FromBody] RefreshRequest req)
        {
            var hash = TokenService.Sha256(req.RefreshToken);

            var currentRefreshToken = await _refreshTokenContext.RefreshTokens
                .SingleOrDefaultAsync(token => token.TokenHash == hash
                && (token.RevokedAtUtc == null
                || token.RevokedAtUtc > DateTime.Now));

            if (currentRefreshToken == null)
                return Conflict(new { isTokenValid = false });

            var session = await _sessionService.FindActiveSessionById(currentRefreshToken.SessionId);
            if (session == null)
                return Conflict(new { isSessionValid = false });

            _sessionService?.TouchSession(session);

            var sessionId = session.SessionId;
            var steamId64 = session.OwnerSteamId64;
            var (access, expiresIn) = _tokenService.CreateAccessToken(steamId64, sessionId);

            var (refresh, refreshExp) = _tokenService.CreateRefreshToken();
            var refreshHash = TokenService.Sha256(refresh);

            RefreshToken refreshToken = new()
            {
                SessionId = session.SessionId,
                TokenHash = refreshHash,
                ExpiresAtUtc = refreshExp,
            };

            _refreshTokenContext.RefreshTokens.Add(refreshToken);

            currentRefreshToken.RevokedAtUtc = DateTime.UtcNow;
            await _refreshTokenContext.SaveChangesAsync();

            currentRefreshToken.ReplacedById = refreshToken.Id;
            await _refreshTokenContext.SaveChangesAsync();

            AuthTokensResponse response = new()
            {
                AccessToken = access,
                ExpiresInSeconds = expiresIn,
                RefreshToken = refresh,
            };

            return Ok(response);
        }

        private async Task<Player?> GetPlayer(long steamId64)
        {
            if (steamId64 == 0)
                return null;

            var player = await _authcontext.Players
                .FirstOrDefaultAsync(p => p.PlayerId == steamId64);

            if (player == null)
                return null;

            return player;
        }

        private async Task<Player?> CreatePlayer(long steamId64)
        {
            if (steamId64 == 0)
                return null;

            var player = await _authcontext.Players
                     .FirstOrDefaultAsync(p => p.PlayerId == steamId64);

            if (player != null)
                return null;

            Player newPlayer = new();
            newPlayer.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            newPlayer.PlayerId = steamId64;
            _authcontext.Players.Add(newPlayer);
            await _authcontext.SaveChangesAsync();

            //todo : выделить в отдельный метод
            PlayerInventory newInventory = new();
            newInventory.OwnerSteamId64 = steamId64;
            _playerInventoryContext.PlayerInventories.Add(newInventory);
            await _playerInventoryContext.SaveChangesAsync();

            return newPlayer;
        }
    }
}
