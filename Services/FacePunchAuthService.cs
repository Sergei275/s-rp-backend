using SRp.Models;
using System.Text;
using System.Text.Json;

namespace SRp.Services
{
    public class FacePunchAuthService
    {
        private readonly IHostEnvironment _env;

        public FacePunchAuthService(IHostEnvironment env)
        {
            _env = env;
        }

        private class ValidateAuthTokenResponse
        {
            public long SteamId { get; set; }
            public string Status { get; set; } = null!;
        }

        public async Task<bool> ValidateToken(LoginRequest authInfo)
        {
            if (_env.IsDevelopment())
                return true;

            try
            {
                var http = new System.Net.Http.HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10),
                };

                var data = new Dictionary<string, object>
                {
                    { "steamid", authInfo.SteamId64 },
                    { "token", authInfo.FacepunchToken }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json"
                    );

                var result = await http.PostAsync("https://services.facepunch.com/sbox/auth/token", content);

                if (!result.IsSuccessStatusCode)
                    return false;

                var response = await result.Content.ReadFromJsonAsync<ValidateAuthTokenResponse>();

                return response is not null
                       && string.Equals(response.Status, "ok", StringComparison.OrdinalIgnoreCase)
                       && response.SteamId == authInfo.SteamId64;
            }
            catch
            {
                return false;
            }
        }
    }
}
