using System.Reflection;
using ChatService.Data;
using ChatService.Interfaces;
using ChatService.Services;
using MassTransit;

namespace ChatService.Extensions;

internal static class IServiceCollectionExtensions {
    public static IServiceCollection AddAuth(this IServiceCollection services) {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .AddJsonFile("appsettings.json")
            .Build();

        var cookies = config["Cookies"] ?? throw new Exception("`Cookies` should be defined in `appsettings.json`");

        services
            .AddAuthentication(cookies)
            .AddCookie(cookies);

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddRealtimeConnection(this IServiceCollection services) {
        services.AddSignalR();

        return services;
    }

    public static IServiceCollection AddRabbitMQ(this IServiceCollection services) {
        return services.AddMassTransit(config => {
            var assembly = Assembly.GetExecutingAssembly();

            config.RegisterConsumersFromAssembly(assembly);

            config.UsingRabbitMq((context, rmq) => {
                rmq.ConfigurationReceiveEndpointsFromAssembly(assembly, context);
            });
        });
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services) {
        return services
                .AddScoped<DapperDbConnection>()
                .AddScoped<IChatsService, ChatsService>();
    }
}