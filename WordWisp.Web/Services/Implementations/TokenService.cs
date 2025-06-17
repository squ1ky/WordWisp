using System.Text.Json;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Services.Implementations
{
    public class TokenService : ITokenService
    {
        public string? GetUserIdFromToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                var payload = token.Split('.')[1];
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }
                var jsonBytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                return claims?.GetValueOrDefault("nameid")?.ToString();
            }
            catch
            {
                return null;
            }
        }

        public string? GetUserRoleFromToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                var payload = token.Split('.')[1];
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }
                var jsonBytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                return claims?.GetValueOrDefault("role")?.ToString();
            }
            catch
            {
                return null;
            }
        }

        public string? GetUsernameFromToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                var payload = token.Split('.')[1];
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }
                var jsonBytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                return claims?.GetValueOrDefault("unique_name")?.ToString();
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenValid(string? token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                var payload = token.Split('.')[1];
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }
                var jsonBytes = Convert.FromBase64String(payload);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (claims?.GetValueOrDefault("exp") is JsonElement expElement && expElement.TryGetInt64(out long exp))
                {
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    return expDateTime > DateTime.UtcNow;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
