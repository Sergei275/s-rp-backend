using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRp.Data;
using SRp.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace s_rp_backend.Controllers
{
    [Route("api/auth/characters")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private static readonly Regex RussianNameRegex = new("^[А-Яа-яЁё]{2,}$", RegexOptions.Compiled);

        private readonly CharactersContext _charaterContext;
        private readonly AuthorizationContext _authorizationContext;

        public CharacterController(CharactersContext charaterContext, AuthorizationContext authorizationContext)
        {
            _charaterContext = charaterContext;
            _authorizationContext = authorizationContext;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<CharacterDto>> CreateCharacter([FromBody] CharacterDto characterDto)
        {
            var steamId64 = GetSteamId64();
            if (steamId64 is null)
                return BadRequest(new { message = "[CharacterController] SteamId not Found" });

            var validationMessage = ValidateCharacterDto(characterDto);
            if (validationMessage is not null)
                return BadRequest(new { message = validationMessage });

            var player = await _authorizationContext.Players
                .FirstOrDefaultAsync(currentPlayer => currentPlayer.SteamId64 == steamId64.Value);

            if (player == null)
                return BadRequest(new { message = "[CharacterController] Player not Found" });

            var character = await Create(characterDto, player.Id);

            _charaterContext.Characters.Add(character);
            await _charaterContext.SaveChangesAsync();

            return Ok(character);
        }

        [Authorize]
        [HttpGet("get")]
        public async Task<ActionResult<List<CharacterDto>>> GetCharacters()
        {
            var steamId64 = GetSteamId64();
            if (steamId64 is null)
                return BadRequest(new { message = "[CharacterController] SteamId not Found" });

            var characters = await GetCharactersForSteamId64(steamId64.Value);

            return Ok(characters);
        }

        private async Task<List<CharacterDto>> GetCharactersForSteamId64(long steamId64)
        {
            var characterList = await _charaterContext.Characters
                .Where(character => character.CharacterOwner.SteamId64 == steamId64)
                .ToListAsync();

            List<CharacterDto> findCharacters = new();

            foreach (var character in characterList)
                findCharacters.Add(new CharacterDto
                {
                    CharacterId = character.CharacterId,
                    Name = character.Name,
                    LastName = character.LastName,
                    Age = character.Age
                });

            return findCharacters;
        }

        private async Task<Character> Create(CharacterDto characterDto, int playerId)
        {
            return await Task.FromResult(new Character
            {
                CharacterId = await GetCharacterId(),
                Name = characterDto.Name,
                LastName = characterDto.LastName,
                Age = characterDto.Age,
                PlayerId = playerId
            });
        }

        private async Task<long> GetCharacterId()
        {
            var lastCharacter = await _charaterContext.Characters
                .OrderByDescending(c => c.CharacterId)
                .FirstOrDefaultAsync();

            return lastCharacter != null ? lastCharacter.CharacterId + 1 : 0;
        }

        private long? GetSteamId64()
        {
            var steamIdValue =
                User.FindFirst("steam_id")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (!long.TryParse(steamIdValue, out var steamId64) || steamId64 <= 0)
                return null;

            return steamId64;
        }

        private static string? ValidateCharacterDto(CharacterDto characterDto)
        {
            if (characterDto == null)
                return "[CharacterController] Character data not Found";

            if (string.IsNullOrWhiteSpace(characterDto.Name) || !RussianNameRegex.IsMatch(characterDto.Name))
                return "Имя должно содержать минимум 2 русские буквы без пробелов и спецсимволов.";

            if (string.IsNullOrWhiteSpace(characterDto.LastName) || !RussianNameRegex.IsMatch(characterDto.LastName))
                return "Фамилия должна содержать минимум 2 русские буквы без пробелов и спецсимволов.";

            if (characterDto.Age < 0 || characterDto.Age > 90)
                return "Возраст должен быть в диапазоне от 0 до 90.";

            return null;
        }
    }
}
