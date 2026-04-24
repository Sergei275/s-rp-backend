using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRp.Data;
using SRp.Models;
using System.Security.Claims;

namespace s_rp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly CharactersContext _charaterContext;

        public CharacterController(CharactersContext charaterContext)
        {
            _charaterContext = charaterContext;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateCharacter(CharacterDto characterDto)
        {
            var steamIdValue =
                User.FindFirst("steam_id")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (!long.TryParse(steamIdValue, out var steamId64) || steamId64 <= 0)
                return BadRequest(new { message = "[CharacterController] SteamId not Found" });

            var character = await Create(characterDto);

            _charaterContext.Characters.Add(character);
            await _charaterContext.SaveChangesAsync();

            return Ok(character);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Character>>> GetCharacters()
        {
            var steamIdValue =
                User.FindFirst("steam_id")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (!long.TryParse(steamIdValue, out var steamId64) || steamId64 <= 0)
                return BadRequest(new { message = "[CharacterController] SteamId not Found" });

            var characters = await GetCharactersForSteamId64(steamId64);

            return Ok(characters);
        }

        private async Task<List<Character>> GetCharactersForSteamId64(long steamId64)
        {
            return await _charaterContext.Characters
                .Where(character => character.CharacterOwner.SteamId64 == steamId64)
                .ToListAsync();
        }

        private async Task<Character> Create(CharacterDto characterDto)
        {
            return await Task.FromResult(new Character
            {
                CharacterId = await GetCharacterId(),
                Name = characterDto.Name,
                LastName = characterDto.LastName,
                Age = characterDto.Age
            });
        }

        private async Task<int> GetCharacterId()
        {
            return await _charaterContext.Characters.CountAsync() + 1;
        }
    }
}