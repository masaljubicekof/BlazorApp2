using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorSupabase.App
{
    public class SupaClient
    {
        private readonly string _url;
        private readonly string _anon;
        private readonly HttpClient _http;
        private string? _accessToken;

        public SupaClient(AppConfig cfg)
        {
            _url = cfg.Supabase.Url.TrimEnd('/');
            _anon = cfg.Supabase.AnonKey;
            _http = new HttpClient();
        }

        // ============ AUTH ============
        public async Task<AuthResponse?> Register(string email, string password)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_url}/auth/v1/signup");
            req.Headers.Add("apikey", _anon);
            req.Content = JsonContent.Create(new { email, password });
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<AuthResponse?> Login(string email, string password)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_url}/auth/v1/token?grant_type=password");
            req.Headers.Add("apikey", _anon);
            req.Content = JsonContent.Create(new { email, password });
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var payload = await res.Content.ReadFromJsonAsync<AuthResponse>();
            _accessToken = payload?.AccessToken;
            return payload;
        }

        public string? AccessToken => _accessToken;

        private HttpRequestMessage Rest(HttpMethod method, string path)
        {
            var req = new HttpRequestMessage(method, $"{_url}/rest/v1/{path}");
            req.Headers.Add("apikey", _anon);
            if (!string.IsNullOrEmpty(_accessToken))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return req;
        }

        // ============ TODOS ============
        public async Task<List<Todo>> GetTodos()
        {
            var res = await _http.SendAsync(Rest(HttpMethod.Get, "todos?select=*&order=inserted_at.desc"));
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Todo>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<Todo?> AddTodo(string title, Guid userId)
        {
            var req = Rest(HttpMethod.Post, "todos");
            req.Headers.Add("Prefer", "return=representation");
            req.Content = JsonContent.Create(new { title, user_id = userId, is_done = false });
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<List<Todo>>())?.FirstOrDefault();
        }

        public async Task<Todo?> ToggleTodo(Guid id, bool done)
        {
            var req = Rest(HttpMethod.Patch, $"todos?id=eq.{id}");
            req.Headers.Add("Prefer", "return=representation");
            req.Content = JsonContent.Create(new { is_done = done });
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<List<Todo>>())?.FirstOrDefault();
        }

        public async Task DeleteTodo(Guid id)
        {
            var res = await _http.SendAsync(Rest(HttpMethod.Delete, $"todos?id=eq.{id}"));
            res.EnsureSuccessStatusCode();
        }

        // ============ TODOS by PROJECT ============
        public async Task<List<Todo>> GetTodosByProject(Guid projectId)
        {
            var res = await _http.SendAsync(Rest(HttpMethod.Get, $"todos?select=*&project_id=eq.{projectId}&order=inserted_at.desc"));
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Todo>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<Todo?> AddTodoToProject(string title, Guid userId, Guid projectId)
        {
            var req = Rest(HttpMethod.Post, "todos");
            req.Headers.Add("Prefer", "return=representation");
            req.Content = JsonContent.Create(new { title, user_id = userId, project_id = projectId, is_done = false });
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<List<Todo>>())?.FirstOrDefault();
        }

        // ============ PROJECTS ============
        public async Task<List<Project>> GetProjects()
        {
            var res = await _http.SendAsync(Rest(HttpMethod.Get, "projects?select=*&order=created_at.desc"));
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Project>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        public async Task<Project?> AddProject(string name, Guid userId)
        {
            var req = Rest(HttpMethod.Post, "projects");
            req.Headers.Add("Prefer", "return=representation");
            req.Content = JsonContent.Create(new { name, user_id = userId });
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<List<Project>>())?.FirstOrDefault();
        }

        public async Task DeleteProject(Guid id)
        {
            var res = await _http.SendAsync(Rest(HttpMethod.Delete, $"projects?id=eq.{id}"));
            res.EnsureSuccessStatusCode();
        }

      // ============ NOTES ============
public async Task<List<Note>> GetNotes(Guid? projectId = null)
{
    // Ako je prosleđen projectId → filtrira po projektu, inače vraća sve
    var path = projectId is null
        ? "notes?order=created_at.desc"
        : $"notes?project_id=eq.{projectId}&order=created_at.desc";

    var req = Rest(HttpMethod.Get, path);
    var res = await _http.SendAsync(req);
    res.EnsureSuccessStatusCode();
    var json = await res.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<List<Note>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
}

public async Task<Note?> AddNote(string title, string body, Guid? projectId, Guid userId)
{
    var req = Rest(HttpMethod.Post, "notes");
    req.Headers.Add("Prefer", "return=representation");
    req.Content = JsonContent.Create(new
    {
        title,
        body,
        user_id = userId,
        project_id = projectId
    });

    var res = await _http.SendAsync(req);
    res.EnsureSuccessStatusCode();

    var created = await res.Content.ReadFromJsonAsync<List<Note>>();
    return created?.FirstOrDefault();
}



        public async Task DeleteNote(Guid id)
        {
            var res = await _http.SendAsync(Rest(HttpMethod.Delete, $"notes?id=eq.{id}"));
            res.EnsureSuccessStatusCode();
        }
    }

    // ===== MODELS =====
    public record AuthResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("user")] SupaUser? User
    );

    public record SupaUser(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("email")] string? Email
    );

    public class Todo
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string title { get; set; } = "";
        public bool is_done { get; set; }
        public DateTime inserted_at { get; set; }
    }

    public class Project
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string name { get; set; } = "";
        public DateTime created_at { get; set; }
    }

    public class Note
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string title { get; set; } = "";
        public string body { get; set; } = "";
        public DateTime created_at { get; set; }
        public Guid project_id { get; set; }
    }
}
