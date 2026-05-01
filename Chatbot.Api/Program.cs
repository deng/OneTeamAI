var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173", "http://127.0.0.1:5173"];

builder.Services
    .AddOptions<ChatbotOptions>()
    .Bind(builder.Configuration.GetSection(ChatbotOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "Chatbot:ApiKey is required.")
    .ValidateOnStart();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=chatbot.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Chatbot API",
        Version = "v1",
        Description = "AI virtual team workspace API with chatbot, team, project, concierge app, conversation, and ticket endpoints.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Bearer",
        In = ParameterLocation.Header,
        Description = "Bearer access token.",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAGUI();
builder.Services.AddSingleton<ChatbotAgentRuntime>();
builder.Services.AddSingleton<IWorkflowTextCompletionService>(sp => sp.GetRequiredService<ChatbotAgentRuntime>());
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<AgentWorkflowOrchestrator>();
builder.Services.AddScoped<AgentWorkflowExecutionService>();
builder.Services.AddScoped<AgentWorkflowWritebackService>();
builder.Services.AddExternalSystemAdapters();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await EnsureDefaultAiMemberTemplatesAsync(dbContext);
}

app.UseCors("frontend");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Chatbot API v1");
    options.RoutePrefix = "swagger";
});

MapSystemEndpoints(app);
MapAuthEndpoints(app);
MapTeamEndpoints(app);
MapProjectEndpoints(app);
MapConciergeAppEndpoints(app);
MapCustomerEndpoints(app);
MapConversationEndpoints(app);
MapTicketEndpoints(app);
MapWorkflowEndpoints(app);
MapIntegrationEndpoints(app);
MapChatEndpoints(app);

app.Run();
