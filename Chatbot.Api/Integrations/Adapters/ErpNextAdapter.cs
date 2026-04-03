using System.Net.Http.Headers;
using System.Text.Json;
using Chatbot.Api.Domain.Entities;
using Chatbot.Api.Domain.Enums;
using Chatbot.Api.Integrations.Internal;
using Chatbot.Api.Integrations.Models;
using Chatbot.Api.Integrations.Providers;

namespace Chatbot.Api.Integrations.Adapters;

public sealed class ErpNextAdapter(IHttpClientFactory httpClientFactory, ILogger<ErpNextAdapter> logger)
    : ICustomerProvider, IProjectProvider, ITicketProvider, ITaskProvider
{
    public ExternalSystemType ExternalSystemType => ExternalSystemType.ErpNext;

    public bool CanHandle(ExternalSystemType externalSystemType) =>
        externalSystemType == ExternalSystemType.ErpNext;

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
        var authHeader = BuildAuthHeader(connection.AuthConfig);
        if (authHeader is null)
        {
            return new IntegrationConnectionTestResult(
                false,
                false,
                "ERPNext authConfig is missing apiKey/apiSecret or bearerToken.",
                DateTimeOffset.UtcNow);
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUri(connection.BaseUrl, "api/method/frappe.auth.get_logged_user"));
            request.Headers.Authorization = authHeader;

            using var response = await client.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new IntegrationConnectionTestResult(
                    true,
                    false,
                    $"ERPNext authentication returned {(int)response.StatusCode}.",
                    DateTimeOffset.UtcNow);
            }

            using var document = JsonDocument.Parse(payload);
            var user = document.RootElement.TryGetProperty("message", out var messageProp)
                ? messageProp.GetString()
                : null;

            return new IntegrationConnectionTestResult(
                true,
                true,
                $"ERPNext authentication succeeded as {user ?? "current user"}.",
                DateTimeOffset.UtcNow);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to test ERPNext connection {ConnectionName}", connection.Name);
            return new IntegrationConnectionTestResult(
                false,
                false,
                $"ERPNext connection test failed: {exception.Message}",
                DateTimeOffset.UtcNow);
        }
    }

    public Task<IReadOnlyList<IntegrationRecordRef>> ListCustomersAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken) =>
        ListResourceAsync(
            connection,
            "Customer",
            ["name", "customer_name", "customer_group"],
            element => new IntegrationRecordRef(
                ReadString(element, "name") ?? "unknown-customer",
                ReadString(element, "customer_name") ?? ReadString(element, "name") ?? "Unknown customer",
                ExternalSystemType.ErpNext,
                ReadString(element, "customer_group")),
            [
                new("erpnext-customer-sample", "ERPNext Sample Customer", ExternalSystemType.ErpNext, "Placeholder customer record")
            ],
            cancellationToken);

    public Task<IReadOnlyList<IntegrationRecordRef>> ListProjectsAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken) =>
        ListResourceAsync(
            connection,
            "Project",
            ["name", "project_name", "status"],
            element => new IntegrationRecordRef(
                ReadString(element, "name") ?? "unknown-project",
                ReadString(element, "project_name") ?? ReadString(element, "name") ?? "Unknown project",
                ExternalSystemType.ErpNext,
                ReadString(element, "status")),
            [
                new("erpnext-project-sample", "ERPNext Sample Project", ExternalSystemType.ErpNext, "Placeholder project record")
            ],
            cancellationToken);

    public Task<IReadOnlyList<IntegrationRecordRef>> ListTicketsAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken) =>
        ListResourceAsync(
            connection,
            "Issue",
            ["name", "subject", "status"],
            element => new IntegrationRecordRef(
                ReadString(element, "name") ?? "unknown-ticket",
                ReadString(element, "subject") ?? ReadString(element, "name") ?? "Unknown issue",
                ExternalSystemType.ErpNext,
                ReadString(element, "status")),
            [
                new("erpnext-ticket-sample", "ERPNext Sample Ticket", ExternalSystemType.ErpNext, "Placeholder ticket record")
            ],
            cancellationToken);

    public Task<IReadOnlyList<IntegrationRecordRef>> ListTasksAsync(
        IntegrationConnectionDescriptor connection,
        CancellationToken cancellationToken) =>
        ListResourceAsync(
            connection,
            "Task",
            ["name", "subject", "status"],
            element => new IntegrationRecordRef(
                ReadString(element, "name") ?? "unknown-task",
                ReadString(element, "subject") ?? ReadString(element, "name") ?? "Unknown task",
                ExternalSystemType.ErpNext,
                ReadString(element, "status")),
            [
                new("erpnext-task-sample", "ERPNext Sample Task", ExternalSystemType.ErpNext, "Placeholder task record")
            ],
            cancellationToken);

    private async Task<IReadOnlyList<IntegrationRecordRef>> ListResourceAsync(
        IntegrationConnectionDescriptor connection,
        string doctype,
        IReadOnlyList<string> fields,
        Func<JsonElement, IntegrationRecordRef> map,
        IReadOnlyList<IntegrationRecordRef> fallback,
        CancellationToken cancellationToken)
    {
        var authHeader = BuildAuthHeader(connection.AuthConfig);
        if (authHeader is null)
        {
            return fallback;
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var url =
                $"api/resource/{Uri.EscapeDataString(doctype)}?limit_page_length=10&fields={Uri.EscapeDataString(JsonSerializer.Serialize(fields))}";
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(connection.BaseUrl, url));
            request.Headers.Authorization = authHeader;

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return fallback;
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(payload);
            if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                return fallback;
            }

            var result = data.EnumerateArray().Select(map).ToList();
            return result.Count > 0 ? result : fallback;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to list ERPNext resource {DocType}", doctype);
            return fallback;
        }
    }

    private static AuthenticationHeaderValue? BuildAuthHeader(string? authConfig)
    {
        var parsed = IntegrationAuthConfigParser.TryParse(authConfig);
        return IntegrationAuthConfigParser.BuildErpNextTokenHeader(
                   IntegrationAuthConfigParser.ReadString(parsed, "apiKey"),
                   IntegrationAuthConfigParser.ReadString(parsed, "apiSecret"))
               ?? IntegrationAuthConfigParser.BuildBearerAuthHeader(
                   IntegrationAuthConfigParser.ReadString(parsed, "bearerToken"));
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static Uri BuildUri(string baseUrl, string relativePath)
    {
        var normalized = baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
        return new Uri(new Uri(normalized), relativePath);
    }
}
