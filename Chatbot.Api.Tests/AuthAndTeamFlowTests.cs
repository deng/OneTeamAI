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
}
