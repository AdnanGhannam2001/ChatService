using ChatService.Data;
using ChatService.Interfaces;
using ChatService.Policies.Handlers;
using ChatService.Policies.Requirements;
using ChatService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using PR2.Shared.Enums;
using static ChatService.Constants.Policies;

namespace ChatService.Extensions;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .AddJsonFile("appsettings.json")
            .Build();

        services.AddDataProtection()
            .SetApplicationName("SocialMedia");

        services
            .AddAuthentication("SocialMediaCookies")
            .AddCookie("SocialMediaCookies");

        services.AddAuthorization(config =>
        {
            config.AddPolicy(UserInChat, policy
                => policy.Requirements.Add(new MembershipRequirement(MemberRoleTypes.Normal)));
            config.AddPolicy(OrganizerInChat, policy
                => policy.Requirements.Add(new MembershipRequirement(MemberRoleTypes.Organizer)));
            config.AddPolicy(AdminInChat, policy
                => policy.Requirements.Add(new MembershipRequirement(MemberRoleTypes.Admin)));
        });

        return services;
    }

    public static IServiceCollection AddRealtimeConnection(this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        return services
                .AddScoped<DapperDbConnection>()
                .AddScoped<IChatsService, ChatsService>()
                .AddScoped<IAuthorizationHandler, MembershipHandler>();
    }
}