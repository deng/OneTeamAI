using Chatbot.Api.Integrations.Adapters;
using Chatbot.Api.Integrations.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Api.Integrations.DependencyInjection;

public static class IntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddExternalSystemAdapters(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<NextcloudAdapter>();
        services.AddSingleton<ErpNextAdapter>();

        services.AddSingleton<IFileKnowledgeProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<NextcloudAdapter>());

        services.AddSingleton<ICustomerProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<ErpNextAdapter>());

        services.AddSingleton<IProjectProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<ErpNextAdapter>());

        services.AddSingleton<ITicketProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<ErpNextAdapter>());

        services.AddSingleton<ITaskProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<ErpNextAdapter>());

        services.AddSingleton<IExternalSystemAdapter>(serviceProvider =>
            serviceProvider.GetRequiredService<NextcloudAdapter>());

        services.AddSingleton<IExternalSystemAdapter>(serviceProvider =>
            serviceProvider.GetRequiredService<ErpNextAdapter>());

        return services;
    }
}
