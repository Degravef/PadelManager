using System.Text.Json;
using Web.Services.Context;

namespace Web.Services;

public abstract class ApiBaseClient
{
    private static readonly JsonSerializerOptions ErrorJsonOptions = new(JsonSerializerDefaults.Web);

    protected readonly HttpClient HttpClient;

    protected ApiBaseClient(HttpClient httpClient, UserContext userContext)
    {
        HttpClient = httpClient;
        HttpClient.DefaultRequestHeaders.Add("X-User-Role", userContext.Role.ToString());
        HttpClient.DefaultRequestHeaders.Add("X-User-Id", userContext.Id);
    }

    protected static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var message = await TryReadMessageAsync(response) ?? $"Unexpected error ({(int)response.StatusCode}).";
        throw new ApiException(response.StatusCode, message);
    }

    private static async Task<string?> TryReadMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var doc = await response.Content.ReadFromJsonAsync<JsonElement>(ErrorJsonOptions);

            if (doc.TryGetProperty("message", out var msg))
                return msg.GetString();

            if (doc.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
                return string.Join(" ", errors.EnumerateArray()
                                              .Select(e => e.TryGetProperty("errorMessage", out var m) ? m.GetString() : null)
                                              .Where(m => !string.IsNullOrWhiteSpace(m)));
        }
        catch (JsonException)
        {
            // le corps n'était pas au format JSON attendu — on retombe sur le message générique
        }
        return null;
    }
}