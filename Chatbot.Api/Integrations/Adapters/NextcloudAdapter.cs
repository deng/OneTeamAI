using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml.Linq;
using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Integrations.Internal;
using Chatbot.Api.Integrations.Models;
using Chatbot.Api.Integrations.Providers;

namespace Chatbot.Api.Integrations.Adapters;

public sealed class NextcloudAdapter(IHttpClientFactory httpClientFactory, ILogger<NextcloudAdapter> logger)
    : IFileKnowledgeProvider
{
    public ExternalSystemType ExternalSystemType => ExternalSystemType.Nextcloud;

    public bool CanHandle(ExternalSystemType externalSystemType) =>
        externalSystemType == ExternalSystemType.Nextcloud;

    public IntegrationConnectionDescriptor BuildDescriptor(IntegrationConnection connection) =>
        new(
            connection.Id,
            connection.TeamId,
            connection.ExternalSystemType,
            connection.Name,
            connection.BaseUrl,
            connection.AuthConfig,
            connection.IsEnabled);

    public async Task<IntegrationConnectionTestResult> TestConnectionAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();

        try
        {
            using var statusRequest = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUri(connection.BaseUrl, "status.php"));
            using var statusResponse = await client.SendAsync(statusRequest, cancellationToken);
            if (!statusResponse.IsSuccessStatusCode)
            {
                return new IntegrationConnectionTestResult(
                    false,
                    false,
                    $"Nextcloud status endpoint returned {(int)statusResponse.StatusCode}",
                    DateTimeOffset.UtcNow);
            }

            var payload = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(payload);
            var version = document.RootElement.TryGetProperty("versionstring", out var versionProp)
                ? versionProp.GetString()
                : null;

            var authHeader = BuildAuthHeader(connection.AuthConfig);
            if (authHeader is null)
            {
                return new IntegrationConnectionTestResult(
                    true,
                    false,
                    "Nextcloud is reachable, but authConfig is missing username/appPassword or bearerToken.",
                    DateTimeOffset.UtcNow,
                    version);
            }

            var username = ReadUsername(connection.AuthConfig);
            if (string.IsNullOrWhiteSpace(username))
            {
                return new IntegrationConnectionTestResult(
                    true,
                    false,
                    "Nextcloud authConfig is missing username, so WebDAV authentication cannot be verified.",
                    DateTimeOffset.UtcNow,
                    version);
            }

            using var davRequest = new HttpRequestMessage(
                new HttpMethod("PROPFIND"),
                BuildDavUri(connection.BaseUrl, username, "/"));
            davRequest.Headers.Authorization = authHeader;
            davRequest.Headers.Add("Depth", "0");
            davRequest.Content = new StringContent(
                """
                <?xml version="1.0"?>
                <d:propfind xmlns:d="DAV:">
                  <d:prop>
                    <d:displayname />
                  </d:prop>
                </d:propfind>
                """);
            davRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            using var davResponse = await client.SendAsync(davRequest, cancellationToken);
            var authenticated = davResponse.StatusCode is HttpStatusCode.MultiStatus or HttpStatusCode.OK;

            return new IntegrationConnectionTestResult(
                true,
                authenticated,
                authenticated
                    ? "Nextcloud connection is reachable and WebDAV authentication succeeded."
                    : $"Nextcloud is reachable, but WebDAV authentication returned {(int)davResponse.StatusCode}.",
                DateTimeOffset.UtcNow,
                version);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to test Nextcloud connection {ConnectionName}", connection.Name);
            return new IntegrationConnectionTestResult(
                false,
                false,
                $"Nextcloud connection test failed: {exception.Message}",
                DateTimeOffset.UtcNow);
        }
    }

    public async Task<IReadOnlyList<FileKnowledgeRecord>> ListFilesAsync(
        IntegrationConnectionDescriptor connection,
        string? folderPath,
        CancellationToken cancellationToken)
    {
        var authHeader = BuildAuthHeader(connection.AuthConfig);
        var username = ReadUsername(connection.AuthConfig);
        if (authHeader is null || string.IsNullOrWhiteSpace(username))
        {
            return CreatePlaceholderFiles(folderPath, "Missing username/appPassword or bearerToken in authConfig.");
        }

        var client = httpClientFactory.CreateClient();
        try
        {
            using var request = new HttpRequestMessage(
                new HttpMethod("PROPFIND"),
                BuildDavUri(connection.BaseUrl, username, folderPath));
            request.Headers.Authorization = authHeader;
            request.Headers.Add("Depth", "1");
            request.Content = new StringContent(
                """
                <?xml version="1.0"?>
                <d:propfind xmlns:d="DAV:">
                  <d:prop>
                    <d:displayname />
                    <d:getlastmodified />
                    <d:getcontenttype />
                    <d:getcontentlength />
                    <d:resourcetype />
                  </d:prop>
                </d:propfind>
                """);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode is not (HttpStatusCode.MultiStatus or HttpStatusCode.OK))
            {
                return CreatePlaceholderFiles(
                    folderPath,
                    $"Nextcloud WebDAV returned {(int)response.StatusCode}.");
            }

            var xml = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = ParseWebDavListing(xml, folderPath).Take(10).ToList();
            return parsed.Count > 0
                ? parsed
                : CreatePlaceholderFiles(folderPath, "Nextcloud returned no visible files for this folder.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to list Nextcloud files for {ConnectionName}", connection.Name);
            return CreatePlaceholderFiles(folderPath, $"Nextcloud request failed: {exception.Message}");
        }
    }

    private static IReadOnlyList<FileKnowledgeRecord> CreatePlaceholderFiles(string? folderPath, string reason)
    {
        return
        [
            new(
                "nextcloud-placeholder-root",
                "Nextcloud Preview Placeholder",
                folderPath ?? "/",
                DateTimeOffset.UtcNow,
                "text/plain",
                reason.Length)
        ];
    }

    private static AuthenticationHeaderValue? BuildAuthHeader(string? authConfig)
    {
        var parsed = IntegrationAuthConfigParser.TryParse(authConfig);
        return IntegrationAuthConfigParser.BuildBasicAuthHeader(
                   IntegrationAuthConfigParser.ReadString(parsed, "username"),
                   IntegrationAuthConfigParser.ReadString(parsed, "appPassword"))
               ?? IntegrationAuthConfigParser.BuildBearerAuthHeader(
                   IntegrationAuthConfigParser.ReadString(parsed, "bearerToken"));
    }

    private static string? ReadUsername(string? authConfig)
    {
        var parsed = IntegrationAuthConfigParser.TryParse(authConfig);
        return IntegrationAuthConfigParser.ReadString(parsed, "username");
    }

    private static Uri BuildUri(string baseUrl, string relativePath)
    {
        return new Uri(new Uri(EnsureTrailingSlash(baseUrl)), relativePath);
    }

    private static Uri BuildDavUri(string baseUrl, string username, string? folderPath)
    {
        var normalizedFolder = string.IsNullOrWhiteSpace(folderPath)
            ? string.Empty
            : folderPath.Trim().Trim('/');
        var encodedUser = Uri.EscapeDataString(username);
        var relative = string.IsNullOrWhiteSpace(normalizedFolder)
            ? $"remote.php/dav/files/{encodedUser}/"
            : $"remote.php/dav/files/{encodedUser}/{normalizedFolder}/";
        return BuildUri(baseUrl, relative);
    }

    private static IEnumerable<FileKnowledgeRecord> ParseWebDavListing(string xml, string? requestedFolder)
    {
        var document = XDocument.Parse(xml);
        XNamespace dav = "DAV:";

        foreach (var response in document.Descendants(dav + "response").Skip(1))
        {
            var href = response.Element(dav + "href")?.Value;
            var prop = response.Descendants(dav + "prop").FirstOrDefault();
            var displayName = prop?.Element(dav + "displayname")?.Value;
            var mimeType = prop?.Element(dav + "getcontenttype")?.Value;
            var sizeValue = prop?.Element(dav + "getcontentlength")?.Value;
            var updatedAtValue = prop?.Element(dav + "getlastmodified")?.Value;
            var hasCollection = prop?.Element(dav + "resourcetype")?.Element(dav + "collection") is not null;

            var normalizedPath = string.IsNullOrWhiteSpace(href)
                ? requestedFolder ?? "/"
                : Uri.UnescapeDataString(href);

            yield return new FileKnowledgeRecord(
                normalizedPath,
                string.IsNullOrWhiteSpace(displayName) ? normalizedPath : displayName,
                normalizedPath,
                DateTimeOffset.TryParse(updatedAtValue, out var updatedAt) ? updatedAt : null,
                hasCollection ? "inode/directory" : mimeType,
                long.TryParse(sizeValue, out var size) ? size : null);
        }
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith('/') ? value : $"{value}/";
}
