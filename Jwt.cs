namespace BlazorSupabase.App
{
    using System.Text.Json;
    public static class Jwt
    {
        public static Guid? GetUserIdFromToken(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            var payload = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(bytes);
            if (json != null && json.TryGetValue("sub", out var sub) && sub is JsonElement el && el.ValueKind == JsonValueKind.String)
                if (Guid.TryParse(el.GetString(), out var g)) return g;
            return null;
        }
    }
}
