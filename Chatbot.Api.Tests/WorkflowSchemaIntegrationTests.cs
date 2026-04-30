using Chatbot.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Chatbot.Api.Tests;

public sealed class WorkflowSchemaIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public WorkflowSchemaIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConversationWorkflow_Should_Use_Structured_Step_And_Summary_Responses()
    {
        _factory.WorkflowTextStub.Reset(
            BuildStepResponse("前台已识别客户核心需求。", "请工单协调员继续整理。"),
            BuildStepResponse("工单协调员已明确优先级和下一步。", "请项目助理输出推进建议。"),
            BuildStepResponse("项目助理已给出范围评估建议。", "请老板确认立项。"),
            BuildSummaryResponse("这是结构化工作流摘要，包含结论、风险和下一步。"));

        using var client = await CreateAuthorizedClientAsync();
        var (teamId, conversationId, startedByMemberId) = await CreateConversationWorkflowContextAsync(client);

        var workflow = await RunConversationWorkflowAsync(client, teamId, conversationId, startedByMemberId);
        var workflowId = workflow?["id"]?.GetValue<string>();

        Assert.Equal("这是结构化工作流摘要，包含结论、风险和下一步。", workflow?["summary"]?.GetValue<string>());
        Assert.Equal("workflow_summary_v1", workflow?["summarySchemaVersion"]?.GetValue<string>());
        Assert.Contains("workflow_summary_v1", workflow?["summaryRawResponse"]?.GetValue<string>());
        Assert.Equal(1, workflow?["summaryAttempts"]?.AsArray().Count);

        var steps = workflow?["steps"]?.AsArray();
        Assert.NotNull(steps);
        Assert.Equal(3, steps!.Count);
        Assert.Equal("前台已识别客户核心需求。", steps[0]?["outputSummary"]?.GetValue<string>());
        Assert.Equal("请工单协调员继续整理。", steps[0]?["handoffSummary"]?.GetValue<string>());
        Assert.Equal("workflow_step_v1", steps[0]?["outputSchemaVersion"]?.GetValue<string>());
        Assert.Contains("workflow_step_v1", steps[0]?["outputRawResponse"]?.GetValue<string>());
        Assert.Equal(1, steps[0]?["outputAttempts"]?.AsArray().Count);
        Assert.Equal("工单协调员已明确优先级和下一步。", steps[1]?["outputSummary"]?.GetValue<string>());
        Assert.Equal("项目助理已给出范围评估建议。", steps[2]?["outputSummary"]?.GetValue<string>());
        Assert.Equal(4, _factory.WorkflowTextStub.CallCount);

        var persistedWorkflow = await LoadWorkflowAsync(workflowId!);
        Assert.Equal("workflow_summary_v1", persistedWorkflow.SummarySchemaVersion);
        Assert.Contains("\"schemaVersion\": \"workflow_summary_v1\"", persistedWorkflow.SummaryRawResponse);
        Assert.Contains("\"Outcome\":\"accepted\"", persistedWorkflow.SummaryAttemptTrace);
        Assert.All(persistedWorkflow.Steps, step =>
        {
            Assert.Equal("workflow_step_v1", step.OutputSchemaVersion);
            Assert.Contains("\"schemaVersion\": \"workflow_step_v1\"", step.OutputRawResponse);
            Assert.Contains("\"Outcome\":\"accepted\"", step.OutputAttemptTrace);
        });
    }

    [Fact]
    public async Task ConversationWorkflow_Should_Retry_When_First_Step_Response_Is_Invalid()
    {
        _factory.WorkflowTextStub.Reset(
            "not-json-response",
            BuildStepResponse("前台重试后返回了合法结构化结果。", "请工单协调员接手。"),
            BuildStepResponse("工单协调员正常输出。", "请项目助理继续。"),
            BuildStepResponse("项目助理正常输出。", "请老板确认。"),
            BuildSummaryResponse("重试后的整体摘要依然成功生成。"));

        using var client = await CreateAuthorizedClientAsync();
        var (teamId, conversationId, startedByMemberId) = await CreateConversationWorkflowContextAsync(client);

        var workflow = await RunConversationWorkflowAsync(client, teamId, conversationId, startedByMemberId);
        var workflowId = workflow?["id"]?.GetValue<string>();

        var steps = workflow?["steps"]?.AsArray();
        Assert.NotNull(steps);
        Assert.Equal("前台重试后返回了合法结构化结果。", steps![0]?["outputSummary"]?.GetValue<string>());
        Assert.Equal("请工单协调员接手。", steps[0]?["handoffSummary"]?.GetValue<string>());
        Assert.Equal("重试后的整体摘要依然成功生成。", workflow?["summary"]?.GetValue<string>());
        Assert.Equal("workflow_step_v1", steps[0]?["outputSchemaVersion"]?.GetValue<string>());
        Assert.Contains("前台重试后返回了合法结构化结果。", steps[0]?["outputRawResponse"]?.GetValue<string>());
        Assert.Equal(2, steps[0]?["outputAttempts"]?.AsArray().Count);
        Assert.Equal("rejected", steps[0]?["outputAttempts"]?[0]?["outcome"]?.GetValue<string>());
        Assert.Equal("accepted", steps[0]?["outputAttempts"]?[1]?["outcome"]?.GetValue<string>());
        Assert.Equal(1, workflow?["summaryAttempts"]?.AsArray().Count);
        Assert.Equal(5, _factory.WorkflowTextStub.CallCount);

        var persistedWorkflow = await LoadWorkflowAsync(workflowId!);
        var firstStep = persistedWorkflow.Steps.OrderBy(step => step.Sequence).First();
        Assert.Equal("workflow_step_v1", firstStep.OutputSchemaVersion);
        Assert.DoesNotContain("not-json-response", firstStep.OutputRawResponse);
        Assert.Contains("前台重试后返回了合法结构化结果。", firstStep.OutputRawResponse);
        var trace = JsonNode.Parse(firstStep.OutputAttemptTrace!)?.AsArray();
        Assert.NotNull(trace);
        Assert.Equal("rejected", trace![0]?["Outcome"]?.GetValue<string>());
        Assert.Equal("accepted", trace[1]?["Outcome"]?.GetValue<string>());
        Assert.False(string.IsNullOrWhiteSpace(trace[0]?["ValidationError"]?.GetValue<string>()));
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"workflow-{suffix}@example.com",
            displayName = $"Workflow User {suffix[..8]}",
            password = "Passw0rd!",
            companyName = "Workflow QA",
        });
        registerResponse.EnsureSuccessStatusCode();

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<JsonObject>();
        var accessToken = authPayload?["accessToken"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private static async Task<(string TeamId, string ConversationId, string StartedByMemberId)> CreateConversationWorkflowContextAsync(HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var teamResponse = await client.PostAsJsonAsync("/api/teams", new
        {
            name = $"Workflow Team {suffix}",
            description = "schema test team",
            brandName = "Workflow QA",
        });
        teamResponse.EnsureSuccessStatusCode();
        var team = await teamResponse.Content.ReadFromJsonAsync<JsonObject>();
        var teamId = team?["id"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(teamId));

        var aiMembers = new List<string>();
        foreach (var member in new[]
        {
            new
            {
                displayName = "Front Desk AI",
                jobTitle = "前台接待",
                responsibilitySummary = "负责接待客户和提炼诉求。",
                templateKey = "front-desk",
                systemPrompt = "输出严格 JSON。",
                knowledgeScope = "需求澄清",
                allowedTools = "conversation-history",
                executableActions = "总结诉求",
                isAutonomous = true,
            },
            new
            {
                displayName = "Ticket Coordinator AI",
                jobTitle = "工单协调员",
                responsibilitySummary = "负责整理需求和优先级。",
                templateKey = "ticket-coordinator",
                systemPrompt = "输出严格 JSON。",
                knowledgeScope = "工单分发",
                allowedTools = "ticketing",
                executableActions = "判断优先级",
                isAutonomous = true,
            },
            new
            {
                displayName = "Project Assistant AI",
                jobTitle = "项目助理",
                responsibilitySummary = "负责输出评估和交接建议。",
                templateKey = "project-assistant",
                systemPrompt = "输出严格 JSON。",
                knowledgeScope = "项目跟进",
                allowedTools = "project-brief",
                executableActions = "生成建议",
                isAutonomous = true,
            }
        })
        {
            var memberResponse = await client.PostAsJsonAsync($"/api/teams/{teamId}/members/ai", member);
            memberResponse.EnsureSuccessStatusCode();
            var memberPayload = await memberResponse.Content.ReadFromJsonAsync<JsonObject>();
            var memberId = memberPayload?["id"]?.GetValue<string>();
            Assert.False(string.IsNullOrWhiteSpace(memberId));
            aiMembers.Add(memberId!);
        }

        var projectResponse = await client.PostAsJsonAsync($"/api/teams/{teamId}/projects", new
        {
            name = $"Workflow Project {suffix}",
            description = "schema test project",
            stageLabel = "intake",
            summary = "测试项目",
            riskSummary = "需尽快评估范围",
            nextSteps = "整理客户需求",
            leadMemberId = aiMembers[2],
        });
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<JsonObject>();
        var projectId = project?["id"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(projectId));

        var conciergeResponse = await client.PostAsJsonAsync($"/api/teams/{teamId}/concierge-apps", new
        {
            projectId,
            name = $"Workflow Concierge {suffix}",
            description = "schema test concierge",
            serviceScope = "企业软件咨询",
            welcomeMessage = "欢迎咨询。",
            intakeGuidance = "请描述你的需求。",
            suggestedPrompts = "我想升级客服系统",
            primaryAiMemberId = aiMembers[0],
            ticketCreationPolicy = "必要时转工单",
            humanHandoffPolicy = "复杂问题转人工",
        });
        conciergeResponse.EnsureSuccessStatusCode();
        var concierge = await conciergeResponse.Content.ReadFromJsonAsync<JsonObject>();
        var conciergeId = concierge?["id"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(conciergeId));

        var conversationResponse = await client.PostAsJsonAsync($"/api/concierge-apps/{conciergeId}/conversations", new
        {
            customerDisplayName = "测试客户",
            customerEmail = $"customer-{suffix}@example.com",
            initialMessage = "希望升级客服系统，支持微信接入和多门店工单流转，并在本月内完成范围评估。",
            autoCreateTicket = false,
        });
        conversationResponse.EnsureSuccessStatusCode();
        var conversation = await conversationResponse.Content.ReadFromJsonAsync<JsonObject>();
        var conversationId = conversation?["id"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(conversationId));

        return (teamId!, conversationId!, aiMembers[0]);
    }

    private static async Task<JsonObject?> RunConversationWorkflowAsync(
        HttpClient client,
        string teamId,
        string conversationId,
        string startedByMemberId)
    {
        var workflowResponse = await client.PostAsJsonAsync($"/api/teams/{teamId}/conversations/{conversationId}/workflows", new
        {
            goal = "验证结构化工作流输出",
            startedByMemberId,
            triggerMode = 0,
        });
        workflowResponse.EnsureSuccessStatusCode();
        return await workflowResponse.Content.ReadFromJsonAsync<JsonObject>();
    }

    private static string BuildStepResponse(string output, string handoff) =>
        $$"""
        {
          "schemaVersion": "workflow_step_v1",
          "status": "completed",
          "output": "{{output}}",
          "handoff": "{{handoff}}"
        }
        """;

    private static string BuildSummaryResponse(string summary) =>
        $$"""
        {
          "schemaVersion": "workflow_summary_v1",
          "summary": "{{summary}}"
        }
        """;

    private async Task<Chatbot.Api.Domain.Entities.AgentWorkflowRun> LoadWorkflowAsync(string workflowId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var workflow = await dbContext.AgentWorkflowRuns
            .Include(run => run.Steps)
            .FirstAsync(run => run.Id == Guid.Parse(workflowId));
        return workflow;
    }
}
