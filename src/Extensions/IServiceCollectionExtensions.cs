using System.Reflection;
using ChatService.Data;
using ChatService.Interfaces;
using ChatService.Policies.Handlers;
using ChatService.Policies.Requirements;
using ChatService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using PR2.Shared.Enums;
using static ChatService.Constants.Policies;

namespace ChatService.Extensions;

internal static class IServiceCollectionExtensions {
    public static IServiceCollection AddAuth(this IServiceCollection services) {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .AddJsonFile("appsettings.json")
            .Build();

        var cookies = config["Cookies"] ?? throw new NullReferenceException("`Cookies` should be defined in `appsettings.json`");

        services
            .AddAuthentication(cookies)
            .AddCookie(cookies);

        services.AddAuthorization(config => {
            config.AddPolicy(UserInChat, policy
                => policy.Requirements.Add(new MembershipRequirement(MemberRoleTypes.Normal)));
            config.AddPolicy(OrganizerInChat, policy
                => policy.Requirements.Add(new MembershipRequirement(MemberRoleTypes.Organizer)));
            config.AddPolicy(AdminInChat, policy
                => policy.Requirements.Add(new MembershipRequirement(MemberRoleTypes.Admin)));
        });

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
                .AddScoped<IChatsService, ChatsService>()
                .AddScoped<IAuthorizationHandler, MembershipHandler>();
    }
}