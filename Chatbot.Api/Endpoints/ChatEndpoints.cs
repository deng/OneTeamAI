partial class Program
{
    static void MapChatEndpoints(WebApplication app)
    {
        app.MapPost("/api/chat", async (
            ChatRequest request,
            ChatbotAgentRuntime runtime,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequestError("message is required", "chat_message_required");
            }

            var sessionId = request.SessionId ?? Guid.NewGuid().ToString("N");
            var session = await runtime.GetOrCreateSessionAsync(sessionId, cancellationToken);
            var chunks = new List<string>();

            await foreach (var update in runtime.Agent.RunStreamingAsync(request.Message, session, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrWhiteSpace(update.Text))
                {
                    chunks.Add(update.Text);
                }
            }

            await runtime.SaveSessionAsync(sessionId, session, cancellationToken);

            return Results.Ok(new ChatResponse(sessionId, string.Concat(chunks)));
        })
        .WithName("CreateChatResponse")
        .WithTags("Chat");

        app.MapPost("/api/chat/stream", async (
            HttpContext httpContext,
            ChatRequest request,
            ChatbotAgentRuntime runtime,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { error = "message is required" }, cancellationToken);
                return;
            }

            var sessionId = request.SessionId ?? Guid.NewGuid().ToString("N");
            var session = await runtime.GetOrCreateSessionAsync(sessionId, cancellationToken);

            httpContext.Response.Headers.ContentType = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection = "keep-alive";

            await WriteSseEventAsync(httpContext, "session", new { sessionId }, cancellationToken);

            try
            {
                await foreach (var update in runtime.Agent.RunStreamingAsync(request.Message, session, cancellationToken: cancellationToken))
                {
                    if (string.IsNullOrWhiteSpace(update.Text))
                    {
                        continue;
                    }

                    await WriteSseEventAsync(httpContext, "delta", new { text = update.Text }, cancellationToken);
                }

                await runtime.SaveSessionAsync(sessionId, session, cancellationToken);
                await WriteSseEventAsync(httpContext, "completed", new { sessionId }, cancellationToken);
            }
            catch (Exception ex)
            {
                await WriteSseEventAsync(httpContext, "error", new { message = ex.Message }, cancellationToken);
            }
        })
        .WithName("CreateChatStream")
        .WithTags("Chat");

        app.MapPost("/api/chat/text-stream", async (
            HttpContext httpContext,
            AiSdkChatRequest request,
            ChatbotAgentRuntime runtime,
            CancellationToken cancellationToken) =>
        {
            var message = GetLatestUserText(request.Messages);
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequestError("A user text message is required.", "chat_message_required");
            }

            var sessionId = string.IsNullOrWhiteSpace(request.Id)
                ? Guid.NewGuid().ToString("N")
                : request.Id;

            var session = await runtime.GetOrCreateSessionAsync(sessionId, cancellationToken);

            httpContext.Response.ContentType = "text/plain; charset=utf-8";
            httpContext.Response.Headers.CacheControl = "no-cache";

            try
            {
                await foreach (var update in runtime.Agent.RunStreamingAsync(message, session, cancellationToken: cancellationToken))
                {
                    if (string.IsNullOrWhiteSpace(update.Text))
                    {
                        continue;
                    }

                    await httpContext.Response.WriteAsync(update.Text, cancellationToken);
                    await httpContext.Response.Body.FlushAsync(cancellationToken);
                }

                await runtime.SaveSessionAsync(sessionId, session, cancellationToken);
                return Results.Empty;
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CreateChatTextStream")
        .WithTags("Chat");

        app.MapAGUI("/agui", app.Services.GetRequiredService<ChatbotAgentRuntime>().Agent)
            .WithName("RunAguiAgent")
            .WithTags("Chat");
    }
}
