using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Chatbot.Api.Tests;

public sealed class AuthAndTeamFlowTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;
    private readonly HttpClient _client;

    public AuthAndTeamFlowTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Should_Reject_Weak_Password()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "weak@example.com",
            displayName = "Weak User",
            password = "weak",
            companyName = "Weak Inc",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("weak_password", payload?["code"]?.GetValue<string>());
    }

    [Fact]
    public async Task Register_CreateTeam_And_ListMyTeams_Should_Succeed()
    {
        var authResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "owner@example.com",
            displayName = "Owner",
            password = "Passw0rd!",
            companyName = "Owner Studio",
        });

        authResponse.EnsureSuccessStatusCode();
        var authPayload = await authResponse.Content.ReadFromJsonAsync<JsonObject>();
        var accessToken = authPayload?["accessToken"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var createTeamResponse = await _client.PostAsJsonAsync("/api/teams", new
        {
            name = "Owner Team",
            description = "Team for integration tests",
            brandName = "Owner Brand",
        });

        createTeamResponse.EnsureSuccessStatusCode();

        var teamsResponse = await _client.GetAsync("/api/teams/me");
        teamsResponse.EnsureSuccessStatusCode();

        var teams = await teamsResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(teams);
        Assert.Contains(teams, item => item?["name"]?.GetValue<string>() == "Owner Team");
    }

    [Fact]
    public async Task NonMember_Should_Not_Access_Team_Members()
    {
        var ownerClient = _client;
        var ownerAuth = await ownerClient.PostAsJsonAsync("/api/auth/register", new
        {
            email = "team-owner@example.com",
            displayName = "Team Owner",
            password = "Passw0rd!",
            companyName = "Owner Co",
        });
        ownerAuth.EnsureSuccessStatusCode();
        var ownerPayload = await ownerAuth.Content.ReadFromJsonAsync<JsonObject>();
        var ownerToken = ownerPayload?["accessToken"]?.GetValue<string>();
        ownerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ownerToken);

        var createTeamResponse = await ownerClient.PostAsJsonAsync("/api/teams", new
        {
            name = "Protected Team",
            description = "Permission checks",
            brandName = "Protected",
        });
        createTeamResponse.EnsureSuccessStatusCode();
        var createdTeam = await createTeamResponse.Content.ReadFromJsonAsync<JsonObject>();
        var teamId = createdTeam?["id"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(teamId));

        using var strangerClient = _factory.CreateClient();
        var strangerAuth = await strangerClient.PostAsJsonAsync("/api/auth/register", new
        {
            email = "stranger@example.com",
            displayName = "Stranger",
            password = "Passw0rd!",
            companyName = "Stranger Co",
        });
        strangerAuth.EnsureSuccessStatusCode();
        var strangerPayload = await strangerAuth.Content.ReadFromJsonAsync<JsonObject>();
        var strangerToken = strangerPayload?["accessToken"]?.GetValue<string>();
        strangerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", strangerToken);

        var forbiddenResponse = await strangerClient.GetAsync($"/api/teams/{teamId}/members");

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        var payload = await forbiddenResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("forbidden", payload?["code"]?.GetValue<string>());
    }

    [Fact]
    public async Task Team_Ai_Member_Template_Library_Should_Support_Custom_Templates()
    {
        var authResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "template-owner@example.com",
            displayName = "Template Owner",
            password = "Passw0rd!",
            companyName = "Template Studio",
        });

        authResponse.EnsureSuccessStatusCode();
        var authPayload = await authResponse.Content.ReadFromJsonAsync<JsonObject>();
        var accessToken = authPayload?["accessToken"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var createTeamResponse = await _client.PostAsJsonAsync("/api/teams", new
        {
            name = "Template Team",
            description = "AI template tests",
            brandName = "Template Brand",
        });
        createTeamResponse.EnsureSuccessStatusCode();
        var createdTeam = await createTeamResponse.Content.ReadFromJsonAsync<JsonObject>();
        var teamId = createdTeam?["id"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(teamId));

        var builtInResponse = await _client.GetAsync($"/api/ai-member-templates?teamId={teamId}");
        builtInResponse.EnsureSuccessStatusCode();
        var builtInTemplates = await builtInResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(builtInTemplates);
        Assert.Contains(builtInTemplates, item => item?["key"]?.GetValue<string>() == "front-desk");

        var createTemplateResponse = await _client.PostAsJsonAsync($"/api/teams/{teamId}/ai-member-templates", new
        {
            label = "售前方案 AI",
            displayName = "售前方案 AI",
            jobTitle = "售前方案顾问",
            responsibilitySummary = "负责梳理客户诉求并输出售前方案建议。",
            permissionBoundary = "只能输出方案建议，不能直接报价。",
            allowedTools = "knowledge.search,project.list",
            executableActions = "梳理诉求,输出方案,识别风险",
            knowledgeScope = "project-docs,playbooks",
            isAutonomous = false,
        });
        createTemplateResponse.EnsureSuccessStatusCode();
        var createdTemplate = await createTemplateResponse.Content.ReadFromJsonAsync<JsonObject>();
        var templateId = createdTemplate?["id"]?.GetValue<string>();
        var templateKey = createdTemplate?["key"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(templateId));
        Assert.False(string.IsNullOrWhiteSpace(templateKey));
        Assert.Equal(teamId, createdTemplate?["teamId"]?.GetValue<string>());
        Assert.False(createdTemplate?["isBuiltIn"]?.GetValue<bool>() ?? true);

        var updateTemplateResponse = await _client.PutAsJsonAsync(
            $"/api/teams/{teamId}/ai-member-templates/{templateId}",
            new
            {
                label = "售前咨询 AI",
                displayName = "售前咨询 AI",
                jobTitle = "售前咨询顾问",
                responsibilitySummary = "负责收集线索并产出售前建议。",
                permissionBoundary = "只能提供建议，不能承诺交付。",
                allowedTools = "knowledge.search,project.list",
                executableActions = "收集线索,输出建议",
                knowledgeScope = "project-docs,playbooks",
                isAutonomous = true,
                isEnabled = true,
                sortOrder = 850
            });
        updateTemplateResponse.EnsureSuccessStatusCode();
        var updatedTemplate = await updateTemplateResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("售前咨询 AI", updatedTemplate?["label"]?.GetValue<string>());
        Assert.True(updatedTemplate?["isAutonomous"]?.GetValue<bool>());

        var teamTemplatesResponse = await _client.GetAsync($"/api/ai-member-templates?teamId={teamId}");
        teamTemplatesResponse.EnsureSuccessStatusCode();
        var teamTemplates = await teamTemplatesResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(teamTemplates);
        Assert.Contains(teamTemplates, item => item?["key"]?.GetValue<string>() == templateKey);

        var globalTemplatesResponse = await _client.GetAsync("/api/ai-member-templates");
        globalTemplatesResponse.EnsureSuccessStatusCode();
        var globalTemplates = await globalTemplatesResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(globalTemplates);
        Assert.DoesNotContain(globalTemplates, item => item?["key"]?.GetValue<string>() == templateKey);

        var disableTemplateResponse = await _client.DeleteAsync($"/api/teams/{teamId}/ai-member-templates/{templateId}");
        disableTemplateResponse.EnsureSuccessStatusCode();
        var disabledTemplate = await disableTemplateResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.False(disabledTemplate?["isEnabled"]?.GetValue<bool>() ?? true);

        var enabledTemplatesResponse = await _client.GetAsync($"/api/ai-member-templates?teamId={teamId}");
        enabledTemplatesResponse.EnsureSuccessStatusCode();
        var enabledTemplates = await enabledTemplatesResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(enabledTemplates);
        Assert.DoesNotContain(enabledTemplates, item => item?["key"]?.GetValue<string>() == templateKey);

        var allTemplatesResponse = await _client.GetAsync($"/api/ai-member-templates?teamId={teamId}&includeDisabled=true");
        allTemplatesResponse.EnsureSuccessStatusCode();
        var allTemplates = await allTemplatesResponse.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(allTemplates);
        Assert.Contains(allTemplates, item => item?["key"]?.GetValue<string>() == templateKey);
    }
}
