using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Chatbot.Api.Integrations.Internal;

internal static class IntegrationAuthConfigParser
{
    public static JsonElement? TryParse(string? authConfig)
    {
        if (string.IsNullOrWhiteSpace(authConfig))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(authConfig);
            return document.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    public static string? ReadString(JsonElement? element, string propertyName)
    {
        if (element is not { ValueKind: JsonValueKind.Object } jsonObject)
        {
            return null;
        }

        if (!jsonObject.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    public static AuthenticationHeaderValue? BuildBasicAuthHeader(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var raw = $"{username}:{password}";
        return new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(raw)));
    }

    public static AuthenticationHeaderValue? BuildBearerAuthHeader(string? token)
    {
        return string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public static AuthenticationHeaderValue? BuildErpNextTokenHeader(string? apiKey, string? apiSecret)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
        {
            return null;
        }

        return new AuthenticationHeaderValue("token", $"{apiKey}:{apiSecret}");
    }
}
